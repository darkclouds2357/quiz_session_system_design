using AutoMapper;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Dtos;
using QuizSessionService.Infrastructure;
using QuizSessionService.Infrastructure.EventStore;
using QuizSessionService.Infrastructure.Query;

namespace QuizSessionService.Domain
{
    public class QuizSession
    {
        private Dictionary<string, AttendedUser> _attendedUsers;
        private IEnumerable<SessionQuestion> _sessionQuestions;
        private readonly IMediator _mediator;
        private readonly IQuizSessionQuery _query;
        private readonly IEventStore _eventStore;


        public QuizSession(IMediator mediator, IQuizSessionQuery query, IEventStore eventStore)
        {
            _sessionQuestions = Enumerable.Empty<SessionQuestion>();
            _attendedUsers = new Dictionary<string, AttendedUser>();
            //_leaderboards = Enumerable.Empty<Leaderboard>();
            Version = 0;
            Id = Guid.NewGuid().ToString();
            this._mediator = mediator;
            this._query = query;
            this._eventStore = eventStore;
        }

        public string Id { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public IReadOnlyList<SessionQuestion> SessionQuestions => _sessionQuestions.ToList();

        public IReadOnlyDictionary<string, AttendedUser> AttendedUsers => _attendedUsers;


        public int Version { get; private set; }
        public bool IsValid => StartTime <= DateTime.UtcNow && DateTime.UtcNow <= EndTime;

        public async Task QuizStartAsync(DateTime endTime, QuestionDto[] questionDtos, CancellationToken cancellationToken = default)
        {
            var sessionQuestions = questionDtos.Select(q => new SessionQuestion(q)).ToList();
            var @event = new QuizSessionStartedEvent(Id, Version + 1)
            {
                EndTime = endTime,
                SessionQuestions = sessionQuestions,
            };

            await ApplyAsync(@event, cancellationToken);

        }

        public async Task<bool> UserJoinQuizSessionAsync(string userId, string userName, CancellationToken cancellationToken = default)
        {
            // TODO Validate if user already attend in quiz session


            // success
            var nextQuestion = _sessionQuestions.FirstRandomValue();

            var @event = new UserAttendedQuizSessionEvent(Id, Version + 1)
            {
                UserId = userId,
                UserName = userName,
                NextQuestion = nextQuestion
            };


            await ApplyAsync(@event, cancellationToken);


            return true;
        }


        public async Task UserSubmitAnswerAsync(string userId, string userName, AnsweredQuestionDto answeredQuestionDto, CancellationToken cancellationToken = default)
        {
            if (!_attendedUsers.TryGetValue(userId, out var attendedUser))
            {
                await UserJoinQuizSessionAsync(userId, userName, cancellationToken);

                attendedUser = _attendedUsers[userId];
            }
            var question = _sessionQuestions.FirstOrDefault(q => q.Id == answeredQuestionDto.SessionQuestionId);
            if (question == null)
            {
                // Validate question not in quiz session
                return;
            }

            //if (attendedUser.IsQuestionAnswered(answeredQuestionDto.SessionQuestionId))
            //{
            //    // Raise event for case question already answered


            //}
            //else
            //{
            var notAnsweredQuestions = _sessionQuestions.Where(q => q.Id != question.Id).ToList();

            var nextQuestion = notAnsweredQuestions.FirstRandomValue();


            var isCorrectAnswer = question.ValidateAnswer(answeredQuestionDto.AnsweredIds, out var score);

            var @event = new UserSubmittedAnswerEvent(Id, Version + 1)
            {
                UserId = userId,
                IsCorrect = isCorrectAnswer,
                SessionQuestionId = question.Id,
                QuestionScore = score,
                CurrentScore = attendedUser.TotalScore + score,
                NextQuestion = nextQuestion,
                AnsweredIds = answeredQuestionDto.AnsweredIds,
            };

            await ApplyAsync(@event, cancellationToken);

            //}

        }

        public async Task CalculateUsersRankAsync(CancellationToken cancellationToken = default)
        {

            // TODO this one should do the calculate in redis for case that has large number of user attened in one quiz session
            // for this case I get all in model for poc

            var leaderboard = _attendedUsers.Values.Select(u => new UserRank
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Score = u.TotalScore
            }).UpdateRank();

            var leaderboardRankUpdatedEvent = new LeaderboardRankUpdatedEvent(Id, Version + 1)
            {
                Leaderboard = leaderboard,
            };
            await ApplyAsync(leaderboardRankUpdatedEvent, cancellationToken);

            foreach (var userRank in leaderboard)
            {
                if (_attendedUsers.TryGetValue(userRank.UserId, out var attendedUser) && attendedUser.Rank != userRank.Rank)
                {
                    var @event = new UserRankChangedEvent(Id, Version + 1)
                    {
                        UserId = userRank.UserId,
                        UserName = userRank.UserName,
                        NewRank = userRank.Rank,
                        PreviousRank = attendedUser.Rank,
                        Score = userRank.Score
                    };
                    await ApplyAsync(@event, cancellationToken);

                }
            }

        }




        private async Task ApplyAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                ((dynamic)this).Apply((dynamic)@event);
            }
            catch (RuntimeBinderException)
            {
            }
            catch (MissingMethodException)
            {
            }

            // because we want to work as fast as possible so the query here will apply 1st, the event store to event store will run in background
            await _eventStore.AddEventAsync(Id, @event, cancellationToken).ConfigureAwait(false);

            await _query.ApplyQueryEventAsync(@event, cancellationToken);
            await _mediator.Publish(@event, cancellationToken);

        }

        public void Apply(UserRankChangedEvent @event)
        {
            var attenedUser = _attendedUsers[@event.UserId];

            attenedUser.UpdateRank(@event.NewRank);
        }


        public void Apply(UserSubmittedAnswerEvent @event)
        {
            var attenedUser = _attendedUsers[@event.UserId];

            attenedUser.AnswerSubmitted(@event.SessionQuestionId, @event.AnsweredIds, @event.QuestionScore, @event.IsCorrect);
            attenedUser.SetNextQuestion(@event.NextQuestion);

        }


        public void Apply(QuizSessionStartedEvent @event)
        {
            Id = @event.StreamId;
            Version = @event.Version;
            StartTime = @event.CreatedAt;
            EndTime = @event.EndTime;
            _sessionQuestions = @event.SessionQuestions;

        }

        public void Apply(UserAttendedQuizSessionEvent @event)
        {
            _attendedUsers ??= new Dictionary<string, AttendedUser>();

            Version = @event.Version;

            var attendedUser = new AttendedUser
            {
                UserId = @event.UserId,
                UserName = @event.UserName,
                AttendedAt = @event.CreatedAt
            };

            _attendedUsers[@event.UserId] = attendedUser;
        }

    }
}

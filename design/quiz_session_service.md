# Quiz Session Service

## Domain Model

### Quiz Session: Aggregate Root

- **id**: Unique identifier for the quiz session (string).
- **start_time**: The time when the quiz session starts (datetime).
- **end_time**: The time when the quiz session ends (datetime).

### Session Questions

- **session_questions**: An array of questions included in the quiz session.
  - **id**: Unique identifier for the session question (string).
  - **question_id**: Identifier of the question sourced from the Questionnaire Service (string).
  - **question_text**: Text of the question sourced from the Questionnaire Service (string).
  - **score**: The score assigned to the question (number).
  - **answers**: An array of possible answers for the question.
    - **id**: Unique identifier for the answer (string).
    - **value**: Text of the answer (string).
    - **is_correct_answer**: Indicates if this answer is correct (boolean).

### Attended Users

- **attended_users**: A hash map where each key is a user ID and each value is a user object representing users who have joined the quiz session.
  - **user_id**: Unique identifier for the user, sourced from JWT claims/authentication (string).
  - **user_name**: Name of the user, sourced from JWT claims/authentication (string).
  - **attended_at**: The time when the user joined the quiz session (datetime).
  - **answered_questions**: An array of questions answered by the user.
    - **question_id**: Identifier of the question answered by the user (string).
    - **submitted_answer_ids**: Identifier of the answers submitted by the user (string).
    - **score**: Total Score obtained for the answered question (number).
  - **current_score**: The cumulative score of the user, computed from answered questions (number).
  - **rank**: The rank of the user within the quiz session, calculated based on the current score compared to other users (number).

### Leaderboard

- **leaderboard**: An array representing the leaderboard, sorted by user scores.
  - **user_id**: Unique identifier for the user (string).
  - **user_name**: Name of the user (string).
  - **score**: The user's current score (number).
  - **rank**: The user's rank in the leaderboard (number).

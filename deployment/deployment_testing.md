# POC Project Deployment and Testing

## Deployment

To deploy the project, run the following Docker Compose command in the `./deployment` directory:

```bash
docker-compose up -d
```

## Testing

### 1. Trigger Start-New-Quiz-Command

Use redis-cli to trigger the start of a new quiz session. The JSON string format for the message is as follows:

```json
{
  "endTime": "<UTC ISO string>", // must be in the future, should be one month from today
  "questions": [
    {
      "questionId": "<string value of question in questionnaire>",
      "text": "<string value text of question>",
      "type": "<string value text of question type>",
      "score": "<number value of score of question>",
      "answers": [
        {
          "text": "<string value of answer>",
          "isCorrectAnswer": "<bool value for correct answer>"
        }
      ] // should include more than 4 answers with one correct answer and the rest false
    }
  ]
}
```

Trigger the command with the following Redis command:

```bash
redis-cli publish 'start-new-quiz-session-command' '<message inline JSON string>'
```

### 2.Get Session Question in Redis

Retrieve the session question from Redis with the following commands:

```bash
redis-cli keys quiz:session:*:snap_shot
redis-cli HGET <key> id // case sensitive
```

### 3.Simulate Users Attending the Quiz

Simulate users joining the quiz session using the following curl command:

```bash
curl -X 'POST' \
  'http://localhost:5000/api/v1/quiz/session/{session_id}/join' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json; api-version=1' \
  -d '<body json>'
```

The body JSON format:

```json
{
  "userId": "<unique user id string>",
  "userName": "<any text for user name>"
}
```

> _Note: In a real implementation, the userId and userName should be obtained from the JWT token or header, not included in the body._

Add the number of users that want to attend for the test.

### 4. User Submit Question

To simulate user submissions, follow these steps:

Retrieve the question and answer ID using Redis commands:

```bash
redis-cli HGET quiz:session:<session_id>:attended_users:<user_id> current_exam
```

Submit data with the following curl command:

```bash
curl -X 'PATCH' \
  'http://localhost:5000/api/v1/quiz/session/{session_id}/exam/{question_id}' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json; api-version=1' \
  -d '{
  "user": {
    "userId": "<unique user id string>",
    "userName": "<any text for user name>"
  },
  "answeredIds": [<array of selected question>]
}'
```

> _Note: For this simulation, the current question displayed to the user, whether upon submission or attendance, will be pushed to the user by the notifier service._

### 5. Check the Log in Notifier Service

Check the logs in the notifier service for events pushed to users. The format of the log is as follows:

```plaintext
======================================================================================================
This Log for simulate action after handle push command send Data to client.
1. Event message created from source at: {createdAt}
2. Convert channel: [{channel}] to private/group connection that user register when Subscribe to server
3. Push data payload of event [{eventName}] to client: 
---------
{payload json string}
---------
4. In case that user lost connect, it can call to communication server to get current state of private/group channel 
*for firebase real-time database, just call to firebase real-time database to get current state of channel
======================================================================================================
```

### *Note*

> For this simulation, the message broker uses Redis Pub/Sub to demonstrate the flow of components in the design. In a real implementation, Kafka or RabbitMQ should be used. Similarly, the database should be updated accordingly.

## Redis Query data structure

### key: `quiz:session:{quiz_session_id}:snap_shot`

Data type: hashset
Hash Fields

- `id`: string
- `version`: number
- `start_time`: utc time string
- `end_time`: utc time string
- `questions` json string

### key: `quiz:session:{quiz_session_id}:leaderboard`

Data Type: sorted set
Member format: `{userName}:{userId}`

### key: `quiz:session:{quiz_session_id}:attended_users:{user_id}`

Data type: hashset
Hash Fields

- `id`: string
- `user_name`: string
- `score`: number
- `current_exam`: json string
- `rank`: number
- `previous_rank`: number

### key: `quiz:session:{quiz_session_id}:attended_users:{user_id}:answered_questions`

Data type: set
Member format: `{questionId}:[{string.Join(",", answeredId)}]:{isCorrect}:{score}`

### key: `quiz:session:{quiz_session_id}:event_stores`

Data Type : hash

Hash Fields:

- `stream_id`: string
- `event_name`: string
- `event_assembly_type` :string
- `version`: number
- `created_at`: UTC ISO String
- `payload`: json string

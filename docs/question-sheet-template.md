# Question Sheet Template Endpoint (for Python image generation)

Endpoint: `GET /api/question-sheets/template/{examId}` (requires Bearer JWT)

Purpose: returns all questions, their options, correct answers, and template metadata so an external script (e.g., Python) can render a question sheet image.

Example response shape:
```json
{
  "examId": "A9E7DC13-C9F7-44B0-9D13-148771AB0B1B",
  "examName": "Demo MCQ Test",
  "questionCount": 10,
  "template": {
    "name": "Default OMR Template",
    "optionsPerQuestion": 4
  },
  "questions": [
    {
      "id": "8e2e0e44-3f4f-4e2b-8da5-aaaaaaaaaaaa",
      "questionNumber": 1,
      "text": "Question 1",
      "correctOption": "A",
      "options": [
        { "key": "A", "text": "Option A", "order": 1 },
        { "key": "B", "text": "Option B", "order": 2 },
        { "key": "C", "text": "Option C", "order": 3 },
        { "key": "D", "text": "Option D", "order": 4 }
      ]
    }
  ]
}
```

Notes:
- `optionsPerQuestion` is derived from non-empty options on the question; default seed uses 4.
- `correctOption` is included for downstream rendering/validation.
- Use the `questionCount` and ordered `questions` array to lay out the sheet consistently.
- Each question's `options` list now returns the actual option text from `QuestionOption.Text` (the query explicitly selects option columns to avoid the question text overwriting them during the join).

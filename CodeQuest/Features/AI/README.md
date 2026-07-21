# AI assistant

`Assistant.aspx` is a learner-only, Premium-only page. It sends the current
course/module/chapter context and the last few messages to the server-side
`GoogleAiClient`; the browser never receives the API key. Until conversation
history tables are added to the ERD, the current conversation is kept in the
server session and is cleared when the learner changes chapter context or
clicks Clear chat.

Add these values to Web.config's `appSettings` to enable live responses:

```xml
<add key="CodeQuestGoogleAiApiKey" value="your-server-side-key" />
<add key="CodeQuestGoogleAiModel" value="gemini-3.5-flash" />
<add key="CodeQuestGoogleAiEndpoint" value="https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent" />
```

The Google AI Studio API key must stay server-side and must not be committed to GitHub. Without
the key, the page remains visible to Premium learners but reports the setup
message when they submit a prompt.

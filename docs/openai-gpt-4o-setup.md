# GPT-4o configuration

The default chat model is `gpt-4o` through the OpenAI Chat Completions API.

## Local setup

Provide your OpenAI API key using an environment variable (never commit it):

```bash
export AI__ApiKey="YOUR_OPENAI_API_KEY"
# Alternative: export OPENAI_API_KEY="YOUR_OPENAI_API_KEY"
```

On PowerShell:

```powershell
$env:AI__ApiKey = "YOUR_OPENAI_API_KEY"
```

You can also use .NET user secrets in the Web project:

```bash
dotnet user-secrets init --project AgentForge.Web
dotnet user-secrets set "AI:ApiKey" "YOUR_OPENAI_API_KEY" --project AgentForge.Web
```

The API key is read from `AI:ApiKey` first, then `OPENAI_API_KEY`. It is transmitted as `Authorization: Bearer <key>`. Do not commit credentials.

The `AI` configuration in `AgentForge.Web/appsettings.json` defaults to `OpenAI`, `https://api.openai.com/v1/`, and `gpt-4o`. The existing `ILlmClient` contract still supports text generation only; agent tool calling is future work.

## Verification

Run the app with a valid key and verify a chat request. A missing key should fail with an explicit configuration error. Live API execution and .NET build/tests were not performed in this change.

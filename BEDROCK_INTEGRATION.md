# Bedrock Agent Integration Guide for Frontend

## Overview
The backend provides a Chat API that connects to AWS Bedrock Agent for AI-powered conversations. The agent can create events, register teams, generate fixtures, and answer questions.

## Architecture

```
Frontend (React/Web) → Chat API → Bedrock Agent → Agent Tools → Database
```

## API Endpoint

**Base URL:** `https://sporthub-rng4.onrender.com`

### POST /api/chat/invoke

Send a message to the Bedrock Agent and get a response.

**Request:**
```json
{
  "message": "I want to register my team for a tournament",
  "sessionId": "user-123-session" // Optional, will be generated if not provided
}
```

**Response:**
```json
{
  "sessionId": "user-123-session",
  "response": "Great! I can help you register. Let me check available tournaments...",
  "timestamp": "2026-10-22T10:30:00Z"
}
```

### GET /api/chat/health

Check if Bedrock is configured correctly.

**Response:**
```json
{
  "status": "healthy",
  "bedrockConfigured": true,
  "timestamp": "2026-10-22T10:30:00Z"
}
```

## Frontend Integration Example

### JavaScript/TypeScript

```typescript
interface ChatRequest {
  message: string;
  sessionId?: string;
}

interface ChatResponse {
  sessionId: string;
  response: string;
  timestamp: string;
}

async function sendMessage(message: string, sessionId?: string): Promise<ChatResponse> {
  const response = await fetch('https://sporthub-rng4.onrender.com/api/chat/invoke', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      message,
      sessionId
    })
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
}

// Usage
const result = await sendMessage("Show me upcoming tournaments");
console.log(result.response);

// With session (for conversation continuity)
const result2 = await sendMessage("Register my team", result.sessionId);
```

### React Hook Example

```typescript
import { useState } from 'react';

export function useBedrockChat() {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sendMessage = async (message: string) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch('https://sporthub-rng4.onrender.com/api/chat/invoke', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          message,
          sessionId
        })
      });

      if (!response.ok) {
        throw new Error('Failed to send message');
      }

      const data = await response.json();
      setSessionId(data.sessionId);
      return data.response;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const resetSession = () => {
    setSessionId(null);
  };

  return {
    sendMessage,
    resetSession,
    loading,
    error,
    sessionId
  };
}

// Usage in component
function ChatComponent() {
  const { sendMessage, loading, error } = useBedrockChat();
  const [messages, setMessages] = useState<Array<{role: string, content: string}>>([]);

  const handleSend = async (userMessage: string) => {
    // Add user message to chat
    setMessages(prev => [...prev, { role: 'user', content: userMessage }]);

    // Get AI response
    const aiResponse = await sendMessage(userMessage);

    // Add AI response to chat
    setMessages(prev => [...prev, { role: 'assistant', content: aiResponse }]);
  };

  return (
    // Your chat UI here
  );
}
```

## What the Agent Can Do

The Bedrock Agent has access to these tools via `/api/agent-tools`:

1. **Create Event** (`POST /api/agent-tools/create-event`)
   - Create tournaments with all details
   
2. **Get Events** (`GET /api/agent-tools/events/{eventId}`)
   - Retrieve event information
   
3. **Register Team** (`POST /api/agent-tools/register-team`)
   - Register teams for tournaments
   
4. **Generate Fixtures** (`POST /api/agent-tools/events/{eventId}/generate-fixtures`)
   - Automatically generate match schedules

## Example Conversations

**User:** "Show me upcoming tournaments"
**Agent:** "Here are the upcoming tournaments: ..."

**User:** "I want to register my team"
**Agent:** "Great! What's your team name?"

**User:** "The Eagles"
**Agent:** "Perfect! Which tournament would you like to register for?"

## Session Management

- `sessionId` is optional on first message
- Save the returned `sessionId` and use it for subsequent messages
- Each user should have their own `sessionId` for conversation continuity
- Sessions persist the conversation context with Bedrock

## Error Handling

```typescript
try {
  const response = await sendMessage(message);
  // Handle success
} catch (error) {
  if (error.message.includes('500')) {
    // Bedrock service error
    showError('AI service temporarily unavailable');
  } else if (error.message.includes('400')) {
    // Bad request
    showError('Invalid message format');
  } else {
    // Network error
    showError('Connection failed');
  }
}
```

## Testing

Test the endpoint:
```bash
curl -X POST https://sporthub-rng4.onrender.com/api/chat/invoke \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, what can you do?"}'
```

Check health:
```bash
curl https://sporthub-rng4.onrender.com/api/chat/health
```

## Environment Variables (Backend)

These are already configured in the backend:
```
AWS__BedrockAgentId=amusema_sports_agent-Azjer58UxL
AWS__BedrockAgentAliasId=TSTALIASID
AWS__BedrockAccessKeyId=<configured>
AWS__BedrockSecretAccessKey=<configured>
AWS__Region=eu-west-1
```

## Notes

- The agent is conversational and context-aware
- It can invoke multiple tools in sequence to complete complex tasks
- Responses are streamed from Bedrock and combined into a single response
- WhatsApp integration uses a separate controller but could be migrated to use this same Bedrock agent

## Support

For backend issues or questions:
- Check Render logs at https://dashboard.render.com
- Contact backend team with sessionId for debugging

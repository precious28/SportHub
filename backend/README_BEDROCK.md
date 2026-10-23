# Bedrock AgentCore Harness Integration

## Architecture

Your WhatsApp bot now uses Bedrock AgentCore Harness for intelligent, conversational responses:

```
WhatsApp → Twilio → C# Webhook → Python Service → Bedrock Harness → Gateway → Your API
```

## Components

### 1. Python Bedrock Service (`bedrock_harness_service.py`)
- Handles `invoke_harness` calls using boto3
- Manages session IDs (min 33 chars required by AWS)
- Streams responses from Bedrock

### 2. C# Service (`BedrockHarnessService.cs`)
- Calls Python service via HTTP
- Maintains session per WhatsApp user
- Returns intelligent responses to WhatsApp

### 3. Bedrock Harness
- **ARN:** `arn:aws:bedrock-agentcore:eu-west-1:262683589693:harness/amusema_sports_agent-Azjer58UxL`
- Uses Gateway with your SportHub API tools
- Conversational AI with context retention

## Running Locally

### 1. Install Python Dependencies
```bash
cd backend
pip install -r requirements.txt
```

### 2. Set AWS Credentials
```bash
export AWS_ACCESS_KEY_ID="your-key"
export AWS_SECRET_ACCESS_KEY="your-secret"
export AWS_DEFAULT_REGION="eu-west-1"
```

### 3. Start Python Service
```bash
python bedrock_harness_service.py 5001
```

Service runs on http://localhost:5001

### 4. Start C# Backend
```bash
cd src/SportHub
dotnet run
```

## Deploying to Render

### Option 1: Two Services (Recommended)

**Service 1: Python Bedrock Service**
- **Type:** Web Service
- **Build Command:** `pip install -r backend/requirements.txt`
- **Start Command:** `python backend/bedrock_harness_service.py 10000`
- **Environment Variables:**
  - `AWS_ACCESS_KEY_ID`
  - `AWS_SECRET_ACCESS_KEY`
  - `AWS_DEFAULT_REGION=eu-west-1`

**Service 2: C# API (Existing)**
- Add environment variable:
  - `Bedrock__PythonServiceUrl=https://your-python-service.onrender.com`

### Option 2: Single Service with Supervisor

Create `Procfile`:
```
web: python backend/bedrock_harness_service.py & cd backend/src/SportHub && dotnet run
```

## Testing

### Test Python Service
```bash
curl -X POST http://localhost:5001/invoke \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello", "sessionId": "test123"}'
```

### Test WhatsApp
Send message to your Twilio WhatsApp number:
```
"hi"
"I want to register my team"
"actually, show me fixtures first"
```

The bot should understand context and corrections!

## Key Features

✅ **Conversational** - Understands natural language  
✅ **Context-aware** - Remembers conversation per session  
✅ **Handles corrections** - "my team name is wrong"  
✅ **Task switching** - "cancel, show fixtures instead"  
✅ **Uses your API** - Via Gateway with OpenAPI schema

## Session Management

- Session ID format: `whatsapp-session-{phoneNumber}`
- Minimum 33 characters (padded with zeros)
- Persistent per WhatsApp user
- Bedrock manages conversation history automatically

## Troubleshooting

**Error: "BedrockAgentCore' object has no attribute 'invoke_harness'"**
- Solution: Upgrade boto3: `pip install --upgrade boto3 botocore`

**Error: "Session ID must be at least 33 characters"**
- Solution: Already handled in code with padding

**Empty responses**
- Check AWS credentials
- Verify Harness ARN is correct
- Check Render logs for Python service errors

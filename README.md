# SportHub - AI-Powered Sports Tournament Management System

🏆 **AWS Hackathon Project - 2026**

An intelligent, multi-channel sports tournament management system powered by AWS Bedrock AgentCore, featuring conversational AI via WhatsApp and web interfaces for seamless team registration, fixture scheduling, and tournament management.

---

## 🎯 Problem Statement

Sports tournament organizers struggle with:
- Manual team registration across multiple channels
- Time-consuming fixture scheduling and conflict management
- Delayed communication with coaches and teams
- Repetitive Q&A about rules, fees, and logistics
- Limited accessibility for coaches without web access

## 💡 Our Solution

SportHub provides an AI-powered platform with:
- ✅ **Conversational AI Registration** - Natural language team registration via WhatsApp
- ✅ **Smart Fixture Generation** - Automated scheduling with conflict detection
- ✅ **Multi-Channel Access** - WhatsApp bot + Web API unified backend
- ✅ **Intelligent Q&A** - Context-aware responses powered by AWS Bedrock
- ✅ **Real-Time Updates** - Instant fixture notifications and status updates

---

## 🏗️ Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────┐
│                     USER CHANNELS                        │
├───────────────────────┬─────────────────────────────────┤
│   📱 WhatsApp         │   🌐 Web Dashboard              │
│   (Twilio)            │   (Future: React/Angular)       │
└───────────┬───────────┴───────────────┬─────────────────┘
            │                           │
            v                           v
    ┌───────────────────────────────────────────┐
    │     C# Backend API (.NET 8)               │
    │     (Render.com)                          │
    │  • Events  • Teams  • Fixtures            │
    └───────────┬────────────────┬──────────────┘
                │                │
                v                v
    ┌───────────────────┐  ┌──────────────────┐
    │  AWS RDS MySQL    │  │  AWS Lambda      │
    │  (Database)       │  │  (Python 3.12)   │
    └───────────────────┘  └────────┬─────────┘
                                    │
                                    v
                        ┌───────────────────────┐
                        │  AWS Bedrock          │
                        │  AgentCore Harness    │
                        │  (Claude 3.5 Sonnet)  │
                        └───────────────────────┘
```

### Tech Stack

**Backend**
- .NET 8 ASP.NET Core Web API
- AWS RDS MySQL Database
- Entity Framework Core (code-first approach)

**AI/ML**
- AWS Bedrock AgentCore Harness
- Model: Claude 3.5 Sonnet v2
- AWS Lambda (Python 3.12) - Bedrock proxy service

**Communication**
- Twilio WhatsApp Business API
- TwiML for message formatting

**Hosting**
- Backend: Render.com
- Lambda: AWS eu-west-1
- Database: AWS RDS eu-west-1

---

## 🚀 Key Features

### 1. 🤖 Conversational AI WhatsApp Bot

**Natural Language Interaction:**
```
User: "Hi"
Bot: "👋 Welcome to Amusema Sports! How can I help you today?
     • Register a team
     • Check fixtures
     • View event details"

User: "I want to register my team"
Bot: "Great! Which event would you like to register for?
     1. 🏆 Gauteng Schools Football Festival
     2. ⚽ Johannesburg Youth Soccer Cup
     3. 👩‍⚽ Women Soccer Launch"

User: "Actually, can I check if Women's event is still open?"
Bot: "The Women Soccer Launch is fully booked (8/8 teams).
     Would you like to register for Johannesburg Youth Soccer Cup instead?"
```

**Key Capabilities:**
- ✅ Context awareness across conversation
- ✅ Handles corrections and mind changes
- ✅ Session memory per phone number
- ✅ Validates age groups and eligibility
- ✅ Provides intelligent recommendations

### 2. 📋 Smart Fixture Management

**Automatic Scheduling:**
- Round-robin or custom tournament formats
- Conflict detection (no team plays twice simultaneously)
- Pitch allocation optimization
- Customizable match durations by age group

**Real-Time Queries:**
```
User: "fixtures"
Bot: "📋 Your Upcoming Fixtures
     
     🏆 Women Soccer Launch
     U13 Women's
     ⚽ Phoenix Warriors vs Durban Waves
     ⏰ 08:00 | 📍 Pitch 1
     
     🏆 Women Soccer Launch
     U13 Women's
     ⚽ Phoenix Warriors vs Cape Town Roses
     ⏰ 14:00 | 📍 Pitch 1"
```

### 3. 🎯 Multi-Channel API

**RESTful Endpoints:**
- `POST /api/whatsapp/webhook` - WhatsApp message handling
- `POST /api/chat/invoke` - General web chat
- `POST /api/chat/invoke-event` - Event-specific web chat
- `GET/POST /api/Events` - Event management
- `GET/POST /api/Teams` - Team management
- `GET/POST /api/Fixtures` - Fixture generation & queries
- `GET/POST /api/Registrations` - Team registrations

### 4. 🧠 Intelligent Responses

**Powered by AWS Bedrock AgentCore:**
- Understands context and corrections
- Provides structured data (tables, lists)
- Handles multi-turn conversations
- Graceful fallback for service issues

---

## 📱 WhatsApp Bot Usage

### Getting Started

1. **Join the Sandbox** (if testing):
   - Send `join wheel-thou` to `+1 415 523 8886` on WhatsApp

2. **Start Conversation**:
   - Send `Hi` or any greeting

3. **Follow Natural Conversation**:
   - Bot will guide you through registration
   - Ask questions naturally (no rigid commands)

### Sample Commands

| What to Say | Bot Action |
|-------------|------------|
| "Hi", "Hello" | Shows main menu with options |
| "Register my team" | Starts registration flow |
| "Fixtures" / "Matches" | Shows your team's schedule |
| "Women's event" | Event-specific information |
| "Can I correct...?" | Handles corrections intelligently |

### Features

✅ **Smart Context**: Bot remembers conversation history  
✅ **Flexible Language**: No need for exact commands  
✅ **Error Recovery**: Handles typos and corrections  
✅ **Rich Formatting**: Tables, emojis, structured data  
✅ **Character Limit**: Automatically truncates long responses (1500 char limit)

---

## 🛠️ Installation & Setup

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **AWS Account** with Bedrock access
- **Twilio Account** (free tier works)
- **MySQL Database** (local or AWS RDS)
- **Python 3.12** (for Lambda development)

### 1. Clone Repository

```bash
git clone https://github.com/sportshub-hackerthon/SportHub.git
cd SportHub
```

### 2. Database Setup

**Option A: Run SQL Scripts**

```bash
# Navigate to SQL scripts
cd backend/src/SportHub/Infrastructure/SqlScripts/CreateTable

# Execute scripts in this order:
mysql -u root -p sporthub < CreateEventTable.sql
mysql -u root -p sporthub < CreateTeamsTable.sql
mysql -u root -p sporthub < CreateRegistrationsTable.sql
mysql -u root -p sporthub < CreateFixturesTable.sql
mysql -u root -p sporthub < CreateNotificationsTable.sql
```

**Option B: Use init script**

```bash
mysql -u root -p < backend/init-rds-database.sql
```

### 3. Configure Environment Variables

**For Local Development:**

Create `backend/src/SportHub/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sporthub;User=root;Password=yourpassword;"
  },
  "AWS": {
    "Region": "eu-west-1"
  },
  "Bedrock": {
    "PythonServiceUrl": "http://localhost:5001"
  },
  "Twilio": {
    "AccountSid": "your_twilio_account_sid",
    "AuthToken": "your_twilio_auth_token",
    "WhatsAppFrom": "whatsapp:+14155238886"
  }
}
```

**For Production (Render.com):**

Set these environment variables in Render dashboard:

```bash
# Database
ConnectionStrings__DefaultConnection=Server=your-rds-endpoint;Database=sporthub;...

# AWS (if not using IAM roles)
AWS__Region=eu-west-1

# Bedrock Service
Bedrock__PythonServiceUrl=https://your-lambda-url.amazonaws.com/default/sporthub-bedrock-service

# Twilio
Twilio__AccountSid=your_account_sid
Twilio__AuthToken=your_auth_token
Twilio__WhatsAppFrom=whatsapp:+14155238886
```

### 4. Run Backend Locally

```bash
cd backend/src/SportHub

# Restore dependencies
dotnet restore

# Run application
dotnet run

# API available at: https://localhost:5001 or http://localhost:5000
```

### 5. Deploy AWS Lambda (Bedrock Service)

**Quick Deploy:**

```bash
cd backend

# Create deployment package
bash deploy_lambda.sh

# Follow the instructions to upload to AWS Lambda Console
# Or use AWS CLI (instructions in deploy_lambda.sh output)
```

**Manual Steps:**
1. Go to [AWS Lambda Console](https://console.aws.amazon.com/lambda)
2. Create function: `sporthub-bedrock-service`
3. Runtime: Python 3.12
4. Upload `lambda_deployment.zip`
5. Set timeout: 30 seconds
6. Add Bedrock permissions (see IAM policy below)
7. Add API Gateway trigger (HTTP API)
8. Copy API Gateway URL

**IAM Policy for Lambda:**

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "bedrock-agentcore:InvokeHarness",
                "bedrock-agentcore:InvokeAgentRuntime"
            ],
            "Resource": "arn:aws:bedrock-agentcore:eu-west-1:YOUR_ACCOUNT:harness/YOUR_HARNESS_ID"
        }
    ]
}
```

### 6. Test the System

**Test WhatsApp Bot:**
1. Send "Hi" to your Twilio WhatsApp number
2. Should receive intelligent response from Bedrock

**Test Web API:**
```bash
curl -X POST https://your-backend-url.onrender.com/api/chat/invoke \
  -H "Content-Type: application/json" \
  -d '{"message": "Hi, I want to register my team", "sessionId": "test-123"}'
```

---

## 📊 Database Schema

### Core Tables

**events**
- `id` (PK) - Event identifier
- `name` - Event name
- `sport` - Sport type (e.g., "Football")
- `start_date`, `end_date` - Event dates
- `location` - Venue name
- `age_groups` - Comma-separated (e.g., "U11,U13,U15")
- `max_teams` - Maximum teams allowed
- `registration_fee` - Fee amount

**teams**
- `id` (PK) - Team identifier
- `name` - Team name
- `age_group` - Age category
- `school_name` - School/organization
- `coach_name`, `coach_email`, `coach_phone` - Coach details

**registrations**
- `id` (PK) - Registration identifier
- `event_id` (FK) - References events
- `team_id` (FK) - References teams
- `registration_date` - Timestamp
- `status` - "Confirmed", "Pending", "Cancelled"

**fixtures**
- `id` (PK) - Fixture identifier
- `event_id` (FK) - References events
- `home_team_id`, `away_team_id` (FK) - References teams
- `match_date`, `kick_off_time` - Schedule
- `pitch_id` - Pitch number
- `age_group`, `division` - Match category

**notifications**
- `id` (PK) - Notification identifier
- `recipient`, `message`, `type` - Notification details
- `sent_at`, `status` - Delivery tracking

---

## 🧪 Testing

### Manual Test Checklist

- [ ] WhatsApp registration flow (full journey)
- [ ] Age group validation
- [ ] Event capacity checking (full events)
- [ ] Fixture generation (multiple teams)
- [ ] Fixture queries via WhatsApp
- [ ] Web API chat endpoints
- [ ] Context switching (user changes mind)
- [ ] Error handling (network failures)
- [ ] Character limit truncation (long responses)

### Test Data

**Pre-loaded Events:**
1. Gauteng Schools Football Festival (Nov 15, 2026)
2. Johannesburg Youth Soccer Cup (Dec 5-6, 2026)
3. Women Soccer Launch (Nov 14, 2026) - Full

**Test Phone Number:**
- Any phone number (Twilio sandbox accepts all)

---

## 🎓 Technical Highlights

### AWS Bedrock Integration

**AgentCore Harness:**
- Custom agent created in Bedrock Console
- Harness ARN stored in Lambda function
- Session management with 33+ character IDs
- Streaming response handling

**Key Implementation:**
```python
# Lambda function calls Bedrock
response = bedrock_client.invoke_harness(
    harnessArn=HARNESS_ARN,
    runtimeSessionId=session_id,  # Format: "whatsapp-session-{phone}"
    messages=[{
        'role': 'user',
        'content': [{'text': user_message}]
    }]
)
```

### Clean Architecture

```
SportHub/
├── Domain/           # Entities (Event, Team, Fixture)
├── Application/      # Services, DTOs, Interfaces
├── Infrastructure/   # Data access, EF Core, SQL scripts
└── API/              # Controllers, TwiML responses
```

### Best Practices Implemented

✅ Repository pattern for data access  
✅ Dependency injection throughout  
✅ DTO mapping for API contracts  
✅ Async/await for all I/O operations  
✅ TwiML for WhatsApp message reliability  
✅ Character limit handling (Twilio 1600 char limit)  
✅ JSON case-insensitive deserialization  
✅ Error logging and graceful fallbacks  
✅ Lambda cold start optimization  

---

## 🚀 Deployment

### Backend (Render.com)

1. Connect GitHub repository
2. Set build command: `cd backend/src/SportHub && dotnet build`
3. Set start command: `cd backend/src/SportHub && dotnet run --urls=http://0.0.0.0:$PORT`
4. Add environment variables (see Configuration section)
5. Deploy!

### Lambda (AWS)

1. Package code: `bash deploy_lambda.sh`
2. Upload to AWS Lambda Console
3. Configure timeout (30s) and memory (512MB)
4. Add IAM permissions for Bedrock
5. Create API Gateway HTTP API trigger
6. Copy endpoint URL to C# backend config

---

## 🐛 Troubleshooting

### WhatsApp not responding

**Check:**
1. Render logs: `Bedrock service returned [StatusCode]`
2. Lambda logs in CloudWatch
3. Twilio webhook configuration (must be POST)
4. Environment variable `Bedrock__PythonServiceUrl`

**Common fixes:**
- Lambda cold start (first message may take 5-10s)
- Check IAM permissions on Lambda
- Verify Harness ARN in Lambda code

### "Message body is over limit" error

**Cause:** Twilio WhatsApp has 1600 character limit

**Fix:** Already handled in code (truncates to 1500 chars)

### JSON deserialization errors

**Cause:** Case-sensitive property names

**Fix:** Code uses `PropertyNameCaseInsensitive = true`

---

## 📈 Performance

**Response Times:**
- WhatsApp → Backend: ~50-100ms
- Backend → Lambda: ~2-5s (cold start), ~500ms (warm)
- Lambda → Bedrock: ~3-10s (depends on response length)
- **Total**: 5-15 seconds end-to-end

**Optimizations:**
- Lambda stays warm with provisioned concurrency (optional, costs $)
- Connection pooling for database
- Async operations throughout
- Minimal payload sizes

---

## 🔮 Future Enhancements

### Short Term
- [ ] React dashboard for organizers
- [ ] Live score updates via WhatsApp
- [ ] Team standings and leaderboards
- [ ] Match result submission

### Long Term
- [ ] Multi-language support (Zulu, Afrikaans, Xhosa)
- [ ] WhatsApp Business API (verified number)
- [ ] Redis for session persistence
- [ ] SMS fallback for non-WhatsApp users
- [ ] Payment integration (Paystack/Stripe)
- [ ] Referee assignment automation
- [ ] Mobile app (Flutter)

---

## 👥 Team & Contact

**Organization:** Umdoko Internet  
**Developer:** Precious Mokgoebone  
**Email:** mokgoeboprecious@gmail.com  
**GitHub:** [@sportshub-hackerthon](https://github.com/sportshub-hackerthon/SportHub)

---

## 📜 License

MIT License - Free to use and modify

---

## 🙏 Acknowledgments

- AWS Bedrock Team for AgentCore platform
- Twilio for WhatsApp Business API
- Render.com for hosting platform
- AWS Community for support and resources

---

## 📞 Support & Contributing

**Found a bug?** Open an issue on GitHub  
**Have a feature idea?** Submit a pull request  
**Need help?** Email mokgoeboprecious@gmail.com

---

**Built with ❤️ for AWS Hackathon 2026**

*Empowering sports communities with intelligent AI automation*

---

## 🎬 Live Demo

**WhatsApp Bot:** Send "join wheel-thou" to `+1 415 523 8886`  
**API Endpoint:** `https://sporthub-rng4.onrender.com`  
**Lambda Service:** `https://d5i73b0h36.execute-api.eu-west-1.amazonaws.com/default/sporthub-bedrock-service`

**Test it now!** 🚀

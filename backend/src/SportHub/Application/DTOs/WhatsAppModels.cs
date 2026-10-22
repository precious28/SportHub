using System.Text.Json.Serialization;

namespace SportHub.Application.DTOs;

// Shared models for WhatsApp controllers
public class EventInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string AgeGroups { get; set; } = string.Empty;
}

public class WhatsAppTeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserSession
{
    public bool IsRegistering { get; set; }
    public RegistrationStep Step { get; set; }
    public List<EventInfo> AvailableEvents { get; set; } = new();
    public EventInfo? SelectedEvent { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string CoachEmail { get; set; } = string.Empty;
    public string CoachPhone { get; set; } = string.Empty;
}

public enum RegistrationStep
{
    ChoosingEvent,
    EnteringTeamName,
    EnteringSchoolName,
    EnteringAgeGroup,
    EnteringCoachName,
    EnteringCoachEmail,
    Confirming
}

public class TeamInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CoachPhone { get; set; } = string.Empty;
}

public class FixtureInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("eventId")]
    public int EventId { get; set; }
    
    public string EventName { get; set; } = string.Empty;
    
    [JsonPropertyName("homeTeamName")]
    public string HomeTeam { get; set; } = string.Empty;
    
    [JsonPropertyName("awayTeamName")]
    public string AwayTeam { get; set; } = string.Empty;
    
    [JsonPropertyName("kickOffTime")]
    public string KickOffTime { get; set; } = string.Empty;
    
    // Legacy fields for compatibility
    public DateTime MatchDate { get; set; }
    public string MatchTime { get; set; } = string.Empty;
    public string Pitch { get; set; } = string.Empty;
    
    [JsonPropertyName("pitchId")]
    public int PitchId { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("ageGroup")]
    public string AgeGroup { get; set; } = string.Empty;
    
    [JsonPropertyName("division")]
    public string Division { get; set; } = string.Empty;
}

// Twilio-specific models
public class TwilioWebhookRequest
{
    public string MessageSid { get; set; } = string.Empty;
    public string AccountSid { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int NumMedia { get; set; }
    public string ProfileName { get; set; } = string.Empty;
}

// AWS WhatsApp-specific models
public class AwsWhatsAppWebhookRequest
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
    
    [JsonPropertyName("entry")]
    public List<WhatsAppEntry>? Entry { get; set; }
}

public class WhatsAppEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("changes")]
    public List<WhatsAppChange>? Changes { get; set; }
}

public class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppValue? Value { get; set; }
    
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class WhatsAppValue
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;
    
    [JsonPropertyName("metadata")]
    public WhatsAppMetadata? Metadata { get; set; }
    
    [JsonPropertyName("messages")]
    public List<WhatsAppMessage>? Messages { get; set; }
}

public class WhatsAppMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public WhatsAppTextMessage? Text { get; set; }
    
    [JsonPropertyName("interactive")]
    public WhatsAppInteractive? Interactive { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class WhatsAppTextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class WhatsAppInteractive
{
    [JsonPropertyName("button_reply")]
    public WhatsAppButtonReply? ButtonReply { get; set; }
}

public class WhatsAppButtonReply
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

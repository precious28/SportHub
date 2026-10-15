export type ChatRole = "user" | "assistant";

export interface Registration {
  teamName: string;
  school: string;
  ageGroup: string;
  players: number;
  coach: string;
}

export type RegistrationStatus =
  | "idle"
  | "collecting"
  | "review"
  | "completed"
  | "cancelled";

export interface ChatMessage {
  id: string;
  role: ChatRole;
  content: string;
  timestamp: string;
  registration?: Registration;
}

export interface AgentRequest {
  conversation_id: string;
  event_id: number;
  message: string;
}

export interface AgentResponse {
  conversation_id: string;
  reply: string;
  status: RegistrationStatus;
  registration?: Registration | null;
}

export interface CreateRegistrationRequest {
  eventId: number;
  teamName: string;
  school: string;
  ageGroup: string;
  players: number;
  coach: string;
  conversationId: string;
}

export interface RegistrationResult {
  id: number;
  eventId: number;
  teamId: number;
  status: "registered" | "pending";
  teamName: string;
}
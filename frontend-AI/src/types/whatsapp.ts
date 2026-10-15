export type WhatsAppMessageStatus =
  | "received"
  | "accepted"
  | "sent"
  | "delivered"
  | "read"
  | "failed";

export interface WhatsAppMessage {
  id: string;
  conversationId: string;
  direction: "inbound" | "outbound";
  sender: "coach" | "agent";
  text: string;
  timestamp: string;
  status?: WhatsAppMessageStatus;
}

export interface WhatsAppConversationData {
  id: string;
  coachName: string | null;
  eventId: number;
  messages: WhatsAppMessage[];
}
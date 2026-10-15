import axios from "axios";
import type {
  WhatsAppConversationData,
} from "../types/whatsapp";

const api = axios.create({
  baseURL:
    import.meta.env.VITE_API_URL ||
    "http://localhost:8000",
  timeout: 15000,
});

export async function getWhatsAppConversation(
  conversationId: string
): Promise<WhatsAppConversationData> {
  const response = await api.get<WhatsAppConversationData>(
    `/api/whatsapp/conversations/${encodeURIComponent(
      conversationId
    )}`,
  );

  return response.data;
}
import axios from "axios";

import type {
  AgentRequest,
  AgentResponse,
} from "../types/chat";

import { mockAgentReply } from "./mockAgent";

const API_URL =
  import.meta.env.VITE_API_URL || "http://localhost:8000";

const DEMO_MODE =
  import.meta.env.VITE_DEMO_MODE !== "false";

export async function sendAgentMessage(
  request: AgentRequest
): Promise<AgentResponse> {
  if (DEMO_MODE) {
    return mockAgentReply(request);
  }

  const response = await axios.post<AgentResponse>(
    `${API_URL}/api/agent/chat`,
    request
  );

  return response.data;
}
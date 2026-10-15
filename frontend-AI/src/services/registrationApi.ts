import axios from "axios";

import type {
  CreateRegistrationRequest,
  RegistrationResult,
} from "../types/chat";

const API_URL =
  import.meta.env.VITE_API_URL || "http://localhost:8000";

const api = axios.create({
  baseURL: API_URL,
  timeout: 30000,
});

export async function createRegistration(
  registration: CreateRegistrationRequest
): Promise<RegistrationResult> {
  const response = await api.post<RegistrationResult>(
    "/api/registrations",
    registration,
    {
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  return response.data;
}
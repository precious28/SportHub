/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import {
  Box,
  Container,
  Typography,
  Paper,
  CircularProgress,
  Alert,
} from "@mui/material";

import type { Event } from "../types/event";
import type { ChatMessage as Message } from "../types/chat";

import { getEvent } from "../services/eventApi";
import { sendAgentMessage } from "../services/agentApi";

import EventDetailsCard from "../components/EventDetailsCard";
import ChatMessage from "../components/ChatMessage";
import ChatInput from "../components/ChatInput";
import type { RegistrationStatus } from "../types/chat";
import RegistrationActions from "../components/RegistrationActions";
import type { Registration, RegistrationResult } from "../types/chat";

import { createRegistration } from "../services/registrationApi";
import { isAxiosError } from "axios";
import WhatsAppRegistrationButton from "../components/WhatsAppRegistrationButton";
export default function CoachChatPage() {
  const [event, setEvent] = useState<Event | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(false);
  const [eventLoading, setEventLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [registrationStatus, setRegistrationStatus] =
    useState<RegistrationStatus>("idle");
  const [draftRegistration, setDraftRegistration] =
    useState<Registration | null>(null);

  const [/* savedRegistration */, setSavedRegistration] =
    useState<RegistrationResult | null>(null);

  const [savingRegistration, setSavingRegistration] = useState(false);
  const [conversationId, setConversationId] = useState(() =>
    crypto.randomUUID(),
  );

  // Load event information
  useEffect(() => {
    getEvent(1)
      .then((data) => {
        setEvent(data);

        setMessages([
          {
            id: crypto.randomUUID(),
            role: "assistant",
            content: `Welcome to ${data.name}! How can I help you?`,
            timestamp: new Date().toISOString(),
          },
        ]);
      })
      .catch(() => {
        setError("Failed to load event information.");
      })
      .finally(() => {
        setEventLoading(false);
      });
  }, []);

  // Send a message to the AI agent
  const handleSend = async (text: string) => {
    if (!event || loading) return;

    setError(null);

    const userMessage: Message = {
      id: crypto.randomUUID(),
      role: "user",
      content: text,
      timestamp: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setLoading(true);

    try {
      const response = await sendAgentMessage({
        conversation_id: conversationId,
        event_id: event.id,
        message: text,
      });

      setConversationId(response?.conversation_id as any);
      setRegistrationStatus(response.status);
      if (response.status === "review" && response.registration) {
        setDraftRegistration(response.registration);
      }

      if (response.status === "cancelled") {
        setDraftRegistration(null);
      }

      const aiMessage: Message = {
        id: crypto.randomUUID(),
        role: "assistant",
        content: response.reply,
        timestamp: new Date().toISOString(),
        registration: response.registration ?? undefined,
      };

      setMessages((prev) => [...prev, aiMessage]);
    } catch {
      setError("Unable to contact the AI agent.");
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmRegistration = async () => {
    if (!event || !draftRegistration || savingRegistration) {
      return;
    }

    setSavingRegistration(true);
    setError(null);

    try {
      const result = await createRegistration({
        eventId: event.id,
        teamName: draftRegistration.teamName,
        school: draftRegistration.school,
        ageGroup: draftRegistration.ageGroup,
        players: draftRegistration.players,
        coach: draftRegistration.coach,
        conversationId,
      });

      setSavedRegistration(result);
      setDraftRegistration(null);
      setRegistrationStatus("completed");

      const successMessage: Message = {
        id: crypto.randomUUID(),
        role: "assistant",
        content:
          result.status === "registered"
            ? `Your team ${result.teamName} has been registered successfully! Registration ID: ${result.id}.`
            : `Your registration has been received and is pending approval. Registration ID: ${result.id}.`,
        timestamp: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, successMessage]);
    } catch (error) {
      let message = "Registration failed. Please try again.";

      if (isAxiosError(error)) {
        const detail = error.response?.data?.detail;

        if (typeof detail === "string") {
          message = detail;
        } else if (!error.response) {
          message = "Unable to connect to the registration server.";
        }
      }

      setError(message);
    } finally {
      setSavingRegistration(false);
    }
  };

  const handleCancelRegistration = async () => {
    if (savingRegistration) return;

    setDraftRegistration(null);
    setRegistrationStatus("idle");

    await handleSend("cancel");
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#f5f7fb", py: 4 }}>
      <Container maxWidth="md">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>
          Amusema Sports AI
        </Typography>

        {eventLoading && <CircularProgress />}

        {event && (
          <>
            <EventDetailsCard event={event} />

            <WhatsAppRegistrationButton event={event} />
          </>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Paper
          sx={{
            height: 600,
            display: "flex",
            flexDirection: "column",
            borderRadius: 3,
            overflow: "hidden",
          }}
        >
          <Box sx={{ p: 2, borderBottom: "1px solid #eee" }}>
            <Typography sx={{ fontWeight: 700 }}>AI Coach Assistant</Typography>
          </Box>

          <Box sx={{ flex: 1, overflowY: "auto", p: 2 }}>
            {messages.map((message) => (
              <ChatMessage key={message.id} message={message} />
            ))}

            {loading && (
              <Typography color="text.secondary">AI is typing...</Typography>
            )}
            {registrationStatus === "review" && draftRegistration && (
              <RegistrationActions
                onConfirm={() => void handleConfirmRegistration()}
                onCancel={() => void handleCancelRegistration()}
                disabled={savingRegistration || loading}
              />
            )}
            {savingRegistration && (
              <Box sx={{ display: "flex", alignItems: "center", gap: 2, p: 2 }}>
                <CircularProgress size={20} />
                <Typography variant="body2">
                  Saving team registration...
                </Typography>
              </Box>
            )}
          </Box>

          <ChatInput onSend={handleSend} disabled={loading || !event} />
        </Paper>
      </Container>
    </Box>
  );
}

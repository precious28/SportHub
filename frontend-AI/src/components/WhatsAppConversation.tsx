/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useRef, useState } from "react";
import {
  Alert,
  Avatar,
  Box,
  Card,
  CardHeader,
  Chip,
  CircularProgress,
  Divider,
  Stack,
  Typography,
} from "@mui/material";

import WhatsAppIcon from "@mui/icons-material/WhatsApp";
import RefreshIcon from "@mui/icons-material/Refresh";

import type {
  WhatsAppConversationData,
  WhatsAppMessage,
} from "../types/whatsapp";

import { getWhatsAppConversation } from "../services/whatsappApi";

interface Props {
  conversationId: string;
}

function MessageBubble({
  message,
}: {
  message: WhatsAppMessage;
}) {
  const isCoach = message.sender === "coach";

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: isCoach ? "flex-end" : "flex-start",
      }}
    >
      <Box
        sx={{
          maxWidth: "80%",
          bgcolor: isCoach ? "#DCF8C6" : "#FFFFFF",
          borderRadius: 2,
          px: 2,
          py: 1.5,
          boxShadow: 1,
        }}
      >
        <Typography
          variant="body2"
          sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}
        >
          {message.text}
        </Typography>

        <Stack
          direction="row"
          spacing={1}
          
          sx={{ mt: 1, justifyContent:"flex-end",
          alignItems:"center" }}
        >
          <Typography variant="caption" color="text.secondary">
            {new Date(message.timestamp).toLocaleTimeString([], {
              hour: "2-digit",
              minute: "2-digit",
            })}
          </Typography>

          {!isCoach && message.status && (
            <Typography
              variant="caption"
              color={
                message.status === "failed"
                  ? "error"
                  : "text.secondary"
              }
            >
              {message.status}
            </Typography>
          )}
        </Stack>
      </Box>
    </Box>
  );
}

export default function WhatsAppConversation({
  conversationId,
}: Props) {
  const [conversation, setConversation] =
    useState<WhatsAppConversationData | null>(null);

  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const bottomRef = useRef<HTMLDivElement>(null);
  const requestInFlight = useRef(false);

  useEffect(() => {
    let active = true;

    setConversation(null);
    setError(null);
    setLoading(true);

    const loadConversation = async () => {
      if (requestInFlight.current) return;

      requestInFlight.current = true;

      try {
        const data = await getWhatsAppConversation(
          conversationId
        );

        if (!active) return;

        setConversation(data);
        setError(null);
      } catch {
        if (active) {
          setError("Unable to load WhatsApp messages.");
        }
      } finally {
        requestInFlight.current = false;

        if (active) {
          setLoading(false);
          setRefreshing(false);
        }
      }
    };

    void loadConversation();

    const interval = window.setInterval(() => {
      if (!document.hidden) {
        setRefreshing(true);
        void loadConversation();
      }
    }, 5000);

    return () => {
      active = false;
      window.clearInterval(interval);
    };
  }, [conversationId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({
      behavior: "smooth",
      block: "nearest",
    });
  }, [conversation?.messages.length]);

  const messages = conversation?.messages ?? [];

  return (
    <Card
      variant="outlined"
      sx={{
        borderRadius: 3,
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
        height: 600,
      }}
    >
      <CardHeader
        avatar={
          <Avatar sx={{ bgcolor: "#25D366" }}>
            <WhatsAppIcon />
          </Avatar>
        }
        title={
          conversation?.coachName ||
          "WhatsApp Conversation"
        }
        subheader="Read-only WhatsApp monitoring"
        action={
          <Chip
            size="small"
            icon={
              refreshing
                ? <CircularProgress size={12} />
                : <RefreshIcon />
            }
            label={error ? "Connection error" : "Auto-refresh"}
            color={error ? "error" : "default"}
          />
        }
      />

      <Divider />

      <Box
        sx={{
          flex: 1,
          overflowY: "auto",
          p: 2,
          bgcolor: "#EAE6DB",
        }}
      >
        {error && (
          <Alert severity="warning" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {loading && (
          <Box sx={{ textAlign: "center", py: 4 }}>
            <CircularProgress />
          </Box>
        )}

        {!loading && !error && messages.length === 0 && (
          <Typography
            color="text.secondary"
            sx={{textAlign:"center"}}
          >
            No messages received yet.
          </Typography>
        )}

        <Stack spacing={2}>
          {messages.map((message) => (
            <MessageBubble
              key={message.id}
              message={message}
            />
          ))}

          <div ref={bottomRef} />
        </Stack>
      </Box>

      <Divider />

      <Box sx={{ p: 2 }}>
        <Typography
          variant="caption"
          color="text.secondary"
        >
          Messages are exchanged through WhatsApp.
          This dashboard is for monitoring only.
        </Typography>
      </Box>
    </Card>
  );
}
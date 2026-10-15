import { Box, Paper, Typography } from "@mui/material";

import type { ChatMessage as Message } from "../types/chat";
import RegistrationSummary from "./RegistrationSummary";

interface Props {
  message: Message;
}

export default function ChatMessage({ message }: Props) {
  const isUser = message.role === "user";

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: isUser ? "flex-end" : "flex-start",
        mb: 2,
      }}
    >
      <Paper
        elevation={0}
        sx={{
          p: 2,
          width: "fit-content",
          maxWidth: "85%",
          bgcolor: isUser ? "#176b54" : "#f1f5f9",
          color: isUser ? "white" : "black",
          borderRadius: 3,
        }}
      >
        <Typography sx={{ whiteSpace: "pre-wrap" }}>
          {message.content}
        </Typography>

        {message.registration && (
          <RegistrationSummary
            registration={message.registration}
          />
        )}

        <Typography
          variant="caption"
          sx={{ display: "block", mt: 1, opacity: 0.7 }}
        >
          {new Date(message.timestamp).toLocaleTimeString()}
        </Typography>
      </Paper>
    </Box>
  );
}
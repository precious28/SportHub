import { useState, type FormEvent } from "react";
import { Box, IconButton, TextField } from "@mui/material";
import SendIcon from "@mui/icons-material/Send";

interface Props {
  onSend: (message: string) => Promise<void>;
  disabled?: boolean;
}

export default function ChatInput({
  onSend,
  disabled = false,
}: Props) {
  const [message, setMessage] = useState("");

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();

    const text = message.trim();

    if (!text || disabled) return;

    setMessage("");
    void onSend(text);
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ display: "flex", gap: 1, p: 2 }}
    >
      <TextField
        fullWidth
        size="small"
        value={message}
        placeholder="Ask Amusema AI..."
        onChange={(e) => setMessage(e.target.value)}
        disabled={disabled}
      />

      <IconButton
        type="submit"
        color="primary"
        disabled={disabled || !message.trim()}
      >
        <SendIcon />
      </IconButton>
    </Box>
  );
}
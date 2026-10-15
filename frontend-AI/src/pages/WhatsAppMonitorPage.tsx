import { Container, Stack, Typography } from "@mui/material";
import WhatsAppConversation from "../components/WhatsAppConversation";

export default function WhatsAppMonitorPage() {
  // Temporary ID for testing.
  // Later, this comes from a conversation list or URL.
  const conversationId = "conv-123";

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" sx={{fontWeight:700}}>
          Amusema WhatsApp Monitor
        </Typography>

        <Typography color="text.secondary">
          Monitor coach conversations handled by
          the Amusema Sports AI agent.
        </Typography>

        <WhatsAppConversation
          conversationId={conversationId}
        />
      </Stack>
    </Container>
  );
}
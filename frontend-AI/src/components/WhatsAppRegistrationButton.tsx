import { Button, Alert, Stack } from "@mui/material";
import WhatsAppIcon from "@mui/icons-material/WhatsApp";
import type { Event } from "../types/event";

interface Props {
  event: Event;
}

export default function WhatsAppRegistrationButton({
  event,
}: Props) {
  const phoneNumber = (
    import.meta.env.VITE_WHATSAPP_NUMBER || ""
  ).replace(/\D/g, "");

  const message =
    `Hi Amusema! I want to register my team ` +
    `for ${event.name}. Event ID: ${event.id}`;

  const whatsappUrl = phoneNumber
    ? `https://wa.me/${phoneNumber}?text=${encodeURIComponent(message)}`
    : "";

  return (
    <Stack spacing={2}>
      {!phoneNumber && (
        <Alert severity="warning">
          WhatsApp business number is not configured.
        </Alert>
      )}

      <Button
        variant="contained"
        color="success"
        size="large"
        startIcon={<WhatsAppIcon />}
        disabled={!phoneNumber}
        component="a"
        href={whatsappUrl || undefined}
        target="_blank"
        rel="noopener noreferrer"
        sx={{
          borderRadius: 3,
          py: 1.5,
          textTransform: "none",
          fontWeight: 700,
        }}
      >
        Register via WhatsApp
      </Button>
    </Stack>
  );
}
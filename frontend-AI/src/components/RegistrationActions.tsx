import { Button, Stack } from "@mui/material";

interface Props {
  onConfirm: () => void;
  onCancel: () => void;
  disabled?: boolean;
}

export default function RegistrationActions({
  onConfirm,
  onCancel,
  disabled = false,
}: Props) {
  return (
    <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
      <Button
        variant="contained"
        color="success"
        disabled={disabled}
        onClick={onConfirm}
      >
        Confirm Registration
      </Button>

      <Button
        variant="outlined"
        color="error"
        disabled={disabled}
        onClick={onCancel}
      >
        Cancel
      </Button>
    </Stack>
  );
}
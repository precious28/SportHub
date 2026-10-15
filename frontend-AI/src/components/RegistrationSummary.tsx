import {
  Card,
  CardContent,
  Typography,
  Stack,
  Divider,
  Box,
} from "@mui/material";

import type { Registration } from "../types/chat";

interface Props {
  registration: Registration;
}

export default function RegistrationSummary({
  registration,
}: Props) {
  const fields = [
    ["Team", registration.teamName],
    ["School", registration.school],
    ["Age Group", registration.ageGroup],
    ["Players", registration.players.toString()],
    ["Coach", registration.coach],
  ];

  return (
    <Card variant="outlined" sx={{ mt: 2, borderRadius: 3 }}>
      <CardContent>
        <Typography variant="h6" sx={{fontWeight:700, mb:2}}>
          Team Registration Summary
        </Typography>

        <Stack spacing={2}>
          {fields.map(([label, value]) => (
            <Box
              key={label}
              sx={{
                display: "flex",
                justifyContent: "space-between",
                gap: 2,
              }}
            >
              <Typography color="text.secondary">
                {label}
              </Typography>

              <Typography sx={{fontWeight:600, textAlign:"right"}}>
                {value}
              </Typography>
            </Box>
          ))}
        </Stack>

        <Divider sx={{ my: 2 }} />

        <Typography variant="caption" color="text.secondary">
          Please verify the information before confirming.
        </Typography>
      </CardContent>
    </Card>
  );
}
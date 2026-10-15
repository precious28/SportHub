import {
  Card,
  CardContent,
  Typography,
  Stack,
  Chip,
  Grid,
  Divider,
} from "@mui/material";

import type { Event } from "../types/event";

interface Props {
  event: Event;
}

export default function EventDetailsCard({ event }: Props) {
  return (
    <Card sx={{ borderRadius: 3, mb: 3 }}>
      <CardContent>
        <Stack spacing={2}>
          <Typography variant="h5" sx={{fontWeight:700}}>
            {event.name}
          </Typography>

          <Chip
            label={event.eventType}
            color="primary"
            sx={{ width: "fit-content" }}
          />

          <Typography>
            📍 {event.venue}
          </Typography>

          <Typography>
            📅 {new Date(event.startDate).toLocaleDateString()}
          </Typography>

          <Typography color="text.secondary">
            {event.description}
          </Typography>

          <Divider />

          <Grid container spacing={2}>
            <Grid size={{ xs: 6, sm: 3 }}>
              <Typography variant="caption">
                Team Capacity
              </Typography>
              <Typography sx={{fontWeight:700}}>
                {event.numberOfTeams}
              </Typography>
            </Grid>

            <Grid size={{ xs: 6, sm: 3 }}>
              <Typography variant="caption">
                Pitches
              </Typography>
              <Typography sx={{fontWeight:700}}>
                {event.numberOfPitches}
              </Typography>
            </Grid>

            <Grid size={{ xs: 6, sm: 3 }}>
              <Typography variant="caption">
                Match Duration
              </Typography>
              <Typography sx={{fontWeight:700}}>
                {event.matchDuration} min
              </Typography>
            </Grid>

            <Grid size={{ xs: 6, sm: 3 }}>
              <Typography variant="caption">
                Break Duration
              </Typography>
              <Typography sx={{fontWeight:700}}>
                {event.breakBetweenMatches} min
              </Typography>
            </Grid>
          </Grid>
        </Stack>
      </CardContent>
    </Card>
  );
}
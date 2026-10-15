import axios from "axios";
import type { Event } from "../types/event";

const API_URL =
  import.meta.env.VITE_API_URL || "http://localhost:8000";

const demoEvent: Event = {
  id: 1,
  name: "Gauteng Schools Football Festival",
  eventType: "Football Festival",
  venue: "TUT Sports Grounds",
  startDate: "2026-11-15T08:00:00",
  endDate: "2026-11-15T17:00:00",
  description: "A football festival for school teams.",
  numberOfTeams: 8,
  numberOfPitches: 2,
  matchDuration: 30,
  breakBetweenMatches: 10,
};

export const getEvent = async (
  eventId: number
): Promise<Event> => {
  if (import.meta.env.VITE_DEMO_MODE !== "false") {
    return { ...demoEvent, id: eventId };
  }

  const response = await axios.get<Event>(
    `${API_URL}/api/events/${eventId}`
  );

  return response.data;
};
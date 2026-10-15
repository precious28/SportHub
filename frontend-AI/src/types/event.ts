export interface Event {
  id: number;
  name: string;
  eventType: string;
  venue: string;
  startDate: string;
  endDate: string;
  description: string;
  numberOfTeams: number;
  numberOfPitches: number;
  matchDuration: number;
  breakBetweenMatches: number;
}
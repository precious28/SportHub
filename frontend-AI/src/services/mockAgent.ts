import type {
  AgentRequest,
  AgentResponse,
  Registration,
  RegistrationStatus,
} from "../types/chat";

type Step =
  | "idle"
  | "team"
  | "school"
  | "age"
  | "players"
  | "coach"
  | "review"
  | "completed";

interface ConversationState {
  step: Step;
  registration: Partial<Registration>;
}

const conversations = new Map<string, ConversationState>();

function createConversation(): ConversationState {
  return {
    step: "idle",
    registration: {},
  };
}

function response(
  conversationId: string,
  reply: string,
  status: RegistrationStatus,
  registration?: Registration
): AgentResponse {
  return {
    conversation_id: conversationId,
    reply,
    status,
    registration: registration ?? null,
  };
}

export async function mockAgentReply(
  request: AgentRequest
): Promise<AgentResponse> {
  // Simulate a network delay
  await new Promise((resolve) => setTimeout(resolve, 500));

  const conversationId = request.conversation_id;
  const message = request.message.trim();
  const lower = message.toLowerCase();

  let state = conversations.get(conversationId);

  if (!state) {
    state = createConversation();
    conversations.set(conversationId, state);
  }

  // Allow cancellation at any point
  if (lower === "cancel" && state.step !== "idle") {
    conversations.delete(conversationId);

    return response(
      conversationId,
      "Your registration has been cancelled. You can start again anytime.",
      "cancelled"
    );
  }

  // STEP 1: Start registration
  if (state.step === "idle" || state.step === "completed") {
    if (lower.includes("register")) {
      state.step = "team";
      state.registration = {};

      return response(
        conversationId,
        "Let's register your team! What is your team name?",
        "collecting"
      );
    }

    if (lower.includes("fixture")) {
      return response(
        conversationId,
        "Fixture information is not connected yet. Please try again later.",
        "idle"
      );
    }

    return response(
      conversationId,
      "Welcome to Amusema Sports AI! I can help you register a team. Type 'register' to begin.",
      "idle"
    );
  }

  // STEP 2: Collect team name
  if (state.step === "team") {
    if (message.length < 2) {
      return response(
        conversationId,
        "Please enter a valid team name.",
        "collecting"
      );
    }

    state.registration.teamName = message;
    state.step = "school";

    return response(
      conversationId,
      `Great! Which school or organisation does ${message} represent?`,
      "collecting"
    );
  }

  // STEP 3: Collect school
  if (state.step === "school") {
    if (message.length < 2) {
      return response(
        conversationId,
        "Please enter a valid school or organisation name.",
        "collecting"
      );
    }

    state.registration.school = message;
    state.step = "age";

    return response(
      conversationId,
      "Which age group is your team registering for? U13 or U15?",
      "collecting"
    );
  }

  // STEP 4: Collect age group
  if (state.step === "age") {
    const age = message.toUpperCase().replace(/\s/g, "");

    if (!["U13", "U15"].includes(age)) {
      return response(
        conversationId,
        "Please select a valid age group: U13 or U15.",
        "collecting"
      );
    }

    state.registration.ageGroup = age;
    state.step = "players";

    return response(
      conversationId,
      "How many players are in your team?",
      "collecting"
    );
  }

  // STEP 5: Collect player count
  if (state.step === "players") {
    const players = Number(message);

    if (
      !Number.isInteger(players) ||
      players < 1 ||
      players > 50
    ) {
      return response(
        conversationId,
        "Please enter a whole number between 1 and 50.",
        "collecting"
      );
    }

    state.registration.players = players;
    state.step = "coach";

    return response(
      conversationId,
      "What is the full name of the team's coach?",
      "collecting"
    );
  }

  // STEP 6: Collect coach name
  if (state.step === "coach") {
    if (message.length < 2) {
      return response(
        conversationId,
        "Please enter the coach's full name.",
        "collecting"
      );
    }

    state.registration.coach = message;
    state.step = "review";

    const registration = state.registration as Registration;

    return response(
      conversationId,
      "Please review your team registration details. Would you like to confirm?",
      "review",
      registration
    );
  }

  // STEP 7: Confirm registration
  if (state.step === "review") {
    if (["confirm", "yes"].includes(lower)) {
      state.step = "completed";

      return response(
        conversationId,
        "Your demo registration has been completed successfully!",
        "completed",
        state.registration as Registration
      );
    }

    return response(
      conversationId,
      "Please type 'confirm' to continue or 'cancel' to stop.",
      "review",
      state.registration as Registration
    );
  }

  return response(
    conversationId,
    "Type 'register' to start a new registration.",
    "idle"
  );
}
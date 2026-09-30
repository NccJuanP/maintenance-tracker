import { apiRequest } from "./client";
import type { UserSummary } from "./types";

export function getUsers(signal?: AbortSignal) {
  return apiRequest<UserSummary[]>("/api/users", { signal });
}
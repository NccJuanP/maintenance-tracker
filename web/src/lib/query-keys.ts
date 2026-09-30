import type { MaintenanceRequestQuery } from "@/lib/api/types";

export const queryKeys = {
  users: ["users"] as const,
  summary: ["summary"] as const,
  requestLists: ["requests", "list"] as const,
  requestList: (query: MaintenanceRequestQuery) => ["requests", "list", query] as const,
  requestDetail: (id: string) => ["requests", "detail", id] as const,
  requestActivity: (id: string) => ["requests", "activity", id] as const,
};
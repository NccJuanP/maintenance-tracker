export type RequestStatus = "Pending" | "InProgress" | "OnHold" | "Resolved" | "Cancelled";
export type Category = "Infrastructure" | "Equipment" | "Software" | "Other";
export type Priority = "Low" | "Medium" | "High" | "Critical";

export interface UserSummary { id: string; name: string; }

export interface MaintenanceRequestListItem {
  id: string;
  title: string;
  category: Category;
  priority: Priority;
  status: RequestStatus;
  assignee: UserSummary | null;
  createdAt: string;
}

export interface MaintenanceRequestDetail extends MaintenanceRequestListItem {
  description: string;
  requester: UserSummary;
  allowedTransitions: RequestStatus[];
  canChangeAssignee: boolean;
}

// Historial: GET /api/maintenance-requests/{id}/activity → PagedResult<ActivityEntry>

interface ActivityBase { id: string; occurredAt: string; performedBy: UserSummary; }

export type ActivityEntry =
  | (ActivityBase & { type: "Created" })
  | (ActivityBase & { type: "StatusChanged"; fromStatus: RequestStatus; toStatus: RequestStatus })
  | (ActivityBase & { type: "AssigneeChanged"; previousAssignee: UserSummary | null; newAssignee: UserSummary });

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RequestSummary { total: number; byStatus: Record<RequestStatus, number>; }

export interface CreateMaintenanceRequestBody {
  title: string;
  description: string;
  category: Category;
  priority: Priority;
}
export interface AssignBody { assigneeId: string; }
export interface ChangeStatusBody { targetStatus: RequestStatus; }

export interface MaintenanceRequestQuery {
  page?: number;
  pageSize?: number;
  status?: RequestStatus;
  priority?: Priority;
  category?: Category;
  search?: string;
  sortDirection?: "asc" | "desc";
}

export interface ApiProblem {
  title: string;
  status: number;
  code: string;                       // ver sección 5.4
  detail?: string;
  errors?: Record<string, string[]>;  // solo en validation_failed
  traceId?: string;
}
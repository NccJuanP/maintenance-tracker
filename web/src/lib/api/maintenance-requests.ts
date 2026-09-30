import { apiRequest } from "./client";
import type {
  ActivityEntry,
  AssignBody,
  ChangeStatusBody,
  CreateMaintenanceRequestBody,
  MaintenanceRequestDetail,
  MaintenanceRequestListItem,
  MaintenanceRequestQuery,
  PagedResult,
  RequestSummary,
} from "./types";

const basePath = "/api/maintenance-requests";

function requestPath(id: string): string {
  return `${basePath}/${encodeURIComponent(id)}`;
}

export function listMaintenanceRequests(query: MaintenanceRequestQuery, signal?: AbortSignal) {
  return apiRequest<PagedResult<MaintenanceRequestListItem>>(basePath, { query: { ...query }, signal });
}

export function getMaintenanceRequest(id: string, signal?: AbortSignal) {
  return apiRequest<MaintenanceRequestDetail>(requestPath(id), { signal });
}

export function getActivity(id: string, page: number, pageSize: number, signal?: AbortSignal) {
  return apiRequest<PagedResult<ActivityEntry>>(`${requestPath(id)}/activity`, {
    query: { page, pageSize },
    signal,
  });
}

export function getSummary(signal?: AbortSignal) {
  return apiRequest<RequestSummary>(`${basePath}/summary`, { signal });
}

export function createMaintenanceRequest(body: CreateMaintenanceRequestBody, userId: string) {
  return apiRequest<MaintenanceRequestDetail>(basePath, { method: "POST", body, userId });
}

export function assignMaintenanceRequest(id: string, body: AssignBody, userId: string) {
  return apiRequest<MaintenanceRequestDetail>(`${requestPath(id)}/assignments`, {
    method: "POST",
    body,
    userId,
  });
}

export function changeMaintenanceRequestStatus(id: string, body: ChangeStatusBody, userId: string) {
  return apiRequest<MaintenanceRequestDetail>(`${requestPath(id)}/status-changes`, {
    method: "POST",
    body,
    userId,
  });
}
import { type QueryClient, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ApiError } from "@/lib/api/client";
import {
  assignMaintenanceRequest,
  changeMaintenanceRequestStatus,
  createMaintenanceRequest,
} from "@/lib/api/maintenance-requests";
import type { CreateMaintenanceRequestBody, MaintenanceRequestDetail, RequestStatus } from "@/lib/api/types";
import { statusLabels } from "@/lib/labels";
import { queryKeys } from "@/lib/query-keys";

interface CreateVariables {
  body: CreateMaintenanceRequestBody;
  actorId: string;
}

interface AssignVariables {
  assigneeId: string;
  actorId: string;
}

interface ChangeStatusVariables {
  targetStatus: RequestStatus;
  actorId: string;
}

export function useCreateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ body, actorId }: CreateVariables) => createMaintenanceRequest(body, actorId),
    onSuccess: (created) => {
      queryClient.setQueryData(queryKeys.requestDetail(created.id), created);
      queryClient.invalidateQueries({ queryKey: queryKeys.requestLists });
      queryClient.invalidateQueries({ queryKey: queryKeys.summary });
    },
  });
}

export function useAssignRequest(requestId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ assigneeId, actorId }: AssignVariables) =>
      assignMaintenanceRequest(requestId, { assigneeId }, actorId),
    onSuccess: (updated) => {
      syncAfterChange(queryClient, updated);
      toast.success(`Responsable actualizado: ${updated.assignee?.name}.`);
    },
    onError: (error) => handleChangeError(queryClient, requestId, error),
  });
}

export function useChangeRequestStatus(requestId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ targetStatus, actorId }: ChangeStatusVariables) =>
      changeMaintenanceRequestStatus(requestId, { targetStatus }, actorId),
    onSuccess: (updated) => {
      syncAfterChange(queryClient, updated);
      toast.success(`Estado actualizado: ${statusLabels[updated.status]}.`);
    },
    onError: (error) => handleChangeError(queryClient, requestId, error),
  });
}

function syncAfterChange(queryClient: QueryClient, updated: MaintenanceRequestDetail) {
  queryClient.setQueryData(queryKeys.requestDetail(updated.id), updated);
  queryClient.invalidateQueries({ queryKey: queryKeys.requestActivity(updated.id) });
  queryClient.invalidateQueries({ queryKey: queryKeys.requestLists });
  queryClient.invalidateQueries({ queryKey: queryKeys.summary });
}

function handleChangeError(queryClient: QueryClient, requestId: string, error: Error) {
  if (error instanceof ApiError && error.status === 409) {
    queryClient.invalidateQueries({ queryKey: queryKeys.requestDetail(requestId) });
    queryClient.invalidateQueries({ queryKey: queryKeys.requestActivity(requestId) });
    toast.error("La solicitud cambió mientras la veías. Ya actualizamos la información; revísala e inténtalo de nuevo.");
    return;
  }

  toast.error(error instanceof ApiError ? error.message : "Ocurrió un error inesperado.");
}
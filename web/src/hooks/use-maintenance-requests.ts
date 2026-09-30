import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listMaintenanceRequests } from "@/lib/api/maintenance-requests";
import type { MaintenanceRequestQuery } from "@/lib/api/types";
import { queryKeys } from "@/lib/query-keys";

export function useMaintenanceRequests(query: MaintenanceRequestQuery) {
  return useQuery({
    queryKey: queryKeys.requestList(query),
    queryFn: ({ signal }) => listMaintenanceRequests(query, signal),
    placeholderData: keepPreviousData,
  });
}
import { keepPreviousData, useInfiniteQuery, useQuery } from "@tanstack/react-query";
import {
  getActivity,
  getMaintenanceRequest,
  listMaintenanceRequests,
} from "@/lib/api/maintenance-requests";
import type { MaintenanceRequestQuery } from "@/lib/api/types";
import { queryKeys } from "@/lib/query-keys";

export const activityPageSize = 10;

export function useMaintenanceRequests(query: MaintenanceRequestQuery) {
  return useQuery({
    queryKey: queryKeys.requestList(query),
    queryFn: ({ signal }) => listMaintenanceRequests(query, signal),
    placeholderData: keepPreviousData,
  });
}

export function useMaintenanceRequest(id: string) {
  return useQuery({
    queryKey: queryKeys.requestDetail(id),
    queryFn: ({ signal }) => getMaintenanceRequest(id, signal),
  });
}

export function useActivity(id: string) {
  return useInfiniteQuery({
    queryKey: queryKeys.requestActivity(id),
    queryFn: ({ pageParam, signal }) => getActivity(id, pageParam, activityPageSize, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined),
  });
}
import { useQuery } from "@tanstack/react-query";
import { getUsers } from "@/lib/api/users";
import { queryKeys } from "@/lib/query-keys";

export function useUsers() {
  return useQuery({
    queryKey: queryKeys.users,
    queryFn: ({ signal }) => getUsers(signal),
    staleTime: Infinity,
  });
}
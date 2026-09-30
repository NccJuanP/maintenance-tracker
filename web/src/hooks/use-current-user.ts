import { useUsers } from "@/hooks/use-users";
import { setStoredUserId, useStoredUserId } from "@/lib/current-user-store";

export function useCurrentUser() {
  const storedUserId = useStoredUserId();
  const { data: users, isPending, isError } = useUsers();

  const user = users?.find((candidate) => candidate.id === storedUserId) ?? users?.[0] ?? null;

  return { user, users, isPending, isError, setUserId: setStoredUserId };
}
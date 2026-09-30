"use client";

import { useCurrentUser } from "@/hooks/use-current-user";

export function UserSelect() {
  const { user, users, isError, setUserId } = useCurrentUser();

  return (
    <label className="flex items-center gap-2 text-sm">
      <span className="text-muted-foreground">Usuario actual</span>
      <select
        className="h-8 rounded-lg border bg-background px-2 text-sm disabled:opacity-60"
        value={user?.id ?? ""}
        onChange={(event) => setUserId(event.target.value)}
        disabled={!users}
      >
        {!users && <option value="">{isError ? "No disponible" : "Cargando..."}</option>}
        {users?.map((candidate) => (
          <option key={candidate.id} value={candidate.id}>
            {candidate.name}
          </option>
        ))}
      </select>
    </label>
  );
}
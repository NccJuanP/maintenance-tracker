"use client";

import { NativeSelect } from "@/components/common/native-select";
import { useCurrentUser } from "@/hooks/use-current-user";

export function UserSelect() {
  const { user, users, isError, setUserId } = useCurrentUser();

  return (
    <label className="flex items-center gap-2 text-sm">
      <span className="text-muted-foreground">Usuario actual</span>
      <NativeSelect
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
      </NativeSelect>
    </label>
  );
}
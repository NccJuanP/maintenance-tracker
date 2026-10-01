"use client";

import { useState } from "react";
import { NativeSelect } from "@/components/common/native-select";
import { Button } from "@/components/ui/button";
import { useCurrentUser } from "@/hooks/use-current-user";
import { useAssignRequest } from "@/hooks/use-request-mutations";
import type { MaintenanceRequestDetail } from "@/lib/api/types";

export function AssigneeControl({ request }: { request: MaintenanceRequestDetail }) {
  const { user, users } = useCurrentUser();
  const assign = useAssignRequest(request.id);
  const currentAssigneeId = request.assignee?.id ?? "";
  const [selectedId, setSelectedId] = useState(currentAssigneeId);

  const canSubmit = Boolean(user) && selectedId !== "" && selectedId !== currentAssigneeId && !assign.isPending;

  function handleAssign() {
    if (!user) {
      return;
    }
    assign.mutate({ assigneeId: selectedId, actorId: user.id });
  }

  return (
    <div className="grid gap-2">
      <label htmlFor="assignee" className="text-sm font-medium">
        Responsable
      </label>
      <div className="flex gap-2">
        <NativeSelect
          id="assignee"
          className="min-w-0 flex-1"
          value={selectedId}
          onChange={(event) => setSelectedId(event.target.value)}
          disabled={!users || assign.isPending}
        >
          <option value="" disabled>
            Selecciona un usuario
          </option>
          {users?.map((candidate) => (
            <option key={candidate.id} value={candidate.id}>
              {candidate.name}
            </option>
          ))}
        </NativeSelect>
        <Button onClick={handleAssign} disabled={!canSubmit}>
          {assign.isPending ? "Guardando..." : request.assignee ? "Reasignar" : "Asignar"}
        </Button>
      </div>
    </div>
  );
}
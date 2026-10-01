"use client";

import { AssigneeControl } from "@/components/requests/assignee-control";
import { StatusActions } from "@/components/requests/status-actions";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useCurrentUser } from "@/hooks/use-current-user";
import type { MaintenanceRequestDetail } from "@/lib/api/types";

export function RequestActionsCard({ request }: { request: MaintenanceRequestDetail }) {
  const { user } = useCurrentUser();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Acciones</CardTitle>
        <CardDescription>
          {user
            ? `Los cambios quedan registrados a nombre de ${user.name}.`
            : "Selecciona un usuario en la parte superior para registrar cambios."}
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-5">
        {request.canChangeAssignee && (
          <AssigneeControl key={request.assignee?.id ?? "none"} request={request} />
        )}
        <StatusActions request={request} />
      </CardContent>
    </Card>
  );
}
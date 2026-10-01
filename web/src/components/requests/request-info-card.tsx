import type { ReactNode } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { MaintenanceRequestDetail } from "@/lib/api/types";
import { formatDateTime } from "@/lib/format";
import { categoryLabels } from "@/lib/labels";

export function RequestInfoCard({ request }: { request: MaintenanceRequestDetail }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Información</CardTitle>
      </CardHeader>
      <CardContent>
        <dl className="grid gap-3">
          <InfoRow label="Responsable">
            {request.assignee?.name ?? <span className="text-muted-foreground">Sin asignar</span>}
          </InfoRow>
          <InfoRow label="Solicitante">{request.requester.name}</InfoRow>
          <InfoRow label="Categoría">{categoryLabels[request.category]}</InfoRow>
          <InfoRow label="Creada">{formatDateTime(request.createdAt)}</InfoRow>
          <InfoRow label="Identificador">
            <span className="font-mono text-xs break-all">{request.id}</span>
          </InfoRow>
        </dl>
      </CardContent>
    </Card>
  );
}

function InfoRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid gap-0.5">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd>{children}</dd>
    </div>
  );
}
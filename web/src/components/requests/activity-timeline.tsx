"use client";

import type { ReactNode } from "react";
import { ErrorState } from "@/components/common/error-state";
import { StatusBadge } from "@/components/requests/request-badges";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useActivity } from "@/hooks/use-maintenance-requests";
import type { ActivityEntry } from "@/lib/api/types";
import { formatDateTime } from "@/lib/format";

export function ActivityTimeline({ requestId }: { requestId: string }) {
  const { data, error, isPending, isError, refetch, hasNextPage, fetchNextPage, isFetchingNextPage } =
    useActivity(requestId);

  let content: ReactNode;

  if (isPending) {
    content = (
      <div role="status" aria-label="Cargando historial" className="space-y-3">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-12 w-full" />
        ))}
      </div>
    );
  } else if (isError) {
    content = <ErrorState title="No se pudo cargar el historial" error={error} onRetry={() => refetch()} />;
  } else {
    const entries = data.pages.flatMap((page) => page.items);
    const totalCount = data.pages[0].totalCount;

    content = (
      <div className="space-y-4">
        <ol className="space-y-4 border-l pl-5">
          {entries.map((entry) => (
            <li key={entry.id} className="relative space-y-1">
              <span
                aria-hidden
                className="absolute top-1.5 -left-[25.5px] size-2.5 rounded-full border-2 border-background bg-primary"
              />
              <div className="flex flex-wrap items-center gap-1.5 text-sm">{describeActivity(entry)}</div>
              <p className="text-xs text-muted-foreground">
                {entry.performedBy.name} · {formatDateTime(entry.occurredAt)}
              </p>
            </li>
          ))}
        </ol>

        <div className="flex flex-wrap items-center justify-between gap-3 text-sm text-muted-foreground">
          <span>
            Mostrando {entries.length} de {totalCount} eventos
          </span>
          {hasNextPage && (
            <Button variant="outline" size="sm" onClick={() => fetchNextPage()} disabled={isFetchingNextPage}>
              {isFetchingNextPage ? "Cargando..." : "Cargar más"}
            </Button>
          )}
        </div>
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Historial de actividad</CardTitle>
        <CardDescription>Del evento más antiguo al más reciente.</CardDescription>
      </CardHeader>
      <CardContent>{content}</CardContent>
    </Card>
  );
}

function describeActivity(entry: ActivityEntry): ReactNode {
  switch (entry.type) {
    case "Created":
      return "Creó la solicitud";
    case "StatusChanged":
      return (
        <>
          Cambió el estado de <StatusBadge status={entry.fromStatus} /> a <StatusBadge status={entry.toStatus} />
        </>
      );
    case "AssigneeChanged":
      return entry.previousAssignee ? (
        <>
          Reasignó de <strong>{entry.previousAssignee.name}</strong> a <strong>{entry.newAssignee.name}</strong>
        </>
      ) : (
        <>
          Asignó a <strong>{entry.newAssignee.name}</strong>
        </>
      );
    default: {
      const unhandled: never = entry;
      return unhandled;
    }
  }
}
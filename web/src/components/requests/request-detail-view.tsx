"use client";

import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { ActivityTimeline } from "@/components/requests/activity-timeline";
import { PriorityBadge, StatusBadge } from "@/components/requests/request-badges";
import { RequestDetailSkeleton } from "@/components/requests/request-detail-skeleton";
import { RequestInfoCard } from "@/components/requests/request-info-card";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useMaintenanceRequest } from "@/hooks/use-maintenance-requests";
import { ApiError } from "@/lib/api/client";
import { RequestActionsCard } from "@/components/requests/request-actions-card";

export function RequestDetailView({ requestId }: { requestId: string }) {
  const { data: request, error, isPending, isError, refetch } = useMaintenanceRequest(requestId);

  if (isPending) {
    return <RequestDetailSkeleton />;
  }

  if (isError) {
    if (error instanceof ApiError && error.status === 404) {
      return (
        <EmptyState
          title="No encontramos esta solicitud"
          description="Puede que el enlace esté mal escrito o que la solicitud no exista."
          action={
            <Link href="/requests" className={buttonVariants({ variant: "outline" })}>
              Volver a solicitudes
            </Link>
          }
        />
      );
    }

    return <ErrorState title="No se pudo cargar la solicitud" error={error} onRetry={() => refetch()} />;
  }

  return (
    <div className="space-y-6">
      <Link href="/requests" className={buttonVariants({ variant: "ghost", size: "sm" })}>
        <ArrowLeft />
        Volver a solicitudes
      </Link>

      <div className="space-y-2">
        <div className="flex flex-wrap items-center gap-2">
          <StatusBadge status={request.status} />
          <PriorityBadge priority={request.priority} />
        </div>
        <h1 className="text-2xl font-semibold tracking-tight">{request.title}</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <aside className="space-y-6 lg:order-last">
          <RequestActionsCard request={request}/>
          <RequestInfoCard request={request} />
        </aside>

        <div className="space-y-6 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Descripción</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="leading-relaxed whitespace-pre-line">{request.description}</p>
            </CardContent>
          </Card>

          <ActivityTimeline requestId={request.id} />
        </div>
      </div>
    </div>
  );
}
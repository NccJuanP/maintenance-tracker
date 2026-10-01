"use client";

import Link from "next/link";
import { RotateCw } from "lucide-react";
import { ErrorState } from "@/components/common/error-state";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useSummary } from "@/hooks/use-maintenance-requests";
import type { RequestStatus } from "@/lib/api/types";
import { formatDateTime } from "@/lib/format";
import { requestStatuses, statusPluralLabels } from "@/lib/labels";
import { cn } from "@/lib/utils";

const statusAccents: Record<RequestStatus, string> = {
  Pending: "border-l-amber-400",
  InProgress: "border-l-blue-500",
  OnHold: "border-l-violet-500",
  Resolved: "border-l-emerald-500",
  Cancelled: "border-l-zinc-400",
};

export function SummaryCards() {
  const { data, error, isPending, isError, isFetching, dataUpdatedAt, refetch } = useSummary();

  if (isPending) {
    return <SummaryCardsSkeleton />;
  }

  if (isError) {
    return <ErrorState title="No se pudo cargar el resumen" error={error} onRetry={() => refetch()} />;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-2 text-sm text-muted-foreground">
        <span>Actualizado: {formatDateTime(new Date(dataUpdatedAt).toISOString())}</span>
        <Button variant="ghost" size="sm" onClick={() => refetch()} disabled={isFetching}>
          <RotateCw className={cn(isFetching && "animate-spin")} />
          Actualizar
        </Button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <SummaryCard href="/requests" label="Total de solicitudes" value={data.total} accent="border-l-foreground" />
        {requestStatuses.map((status) => (
          <SummaryCard
            key={status}
            href={`/requests?status=${status}`}
            label={statusPluralLabels[status]}
            value={data.byStatus[status]}
            total={data.total}
            accent={statusAccents[status]}
          />
        ))}
      </div>
    </div>
  );
}

interface SummaryCardProps {
  href: string;
  label: string;
  value: number;
  total?: number;
  accent: string;
}

function SummaryCard({ href, label, value, total, accent }: SummaryCardProps) {
  const share = total ? Math.round((value / total) * 100) : null;

  return (
    <Link
      href={href}
      className={cn(
        "block space-y-1 rounded-xl border border-l-4 bg-card p-4 transition-colors hover:bg-muted/50",
        accent,
      )}
    >
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-3xl font-semibold tabular-nums">{value}</p>
      {share !== null && <p className="text-xs text-muted-foreground">{share} % del total</p>}
    </Link>
  );
}

function SummaryCardsSkeleton() {
  return (
    <div role="status" aria-label="Cargando resumen" className="space-y-4">
      <Skeleton className="h-8 w-64" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }, (_, index) => (
          <Skeleton key={index} className="h-28 rounded-xl" />
        ))}
      </div>
    </div>
  );
}
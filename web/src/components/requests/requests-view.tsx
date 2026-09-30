"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { Pagination } from "@/components/common/pagination";
import { RequestsFilters } from "@/components/requests/requests-filters";
import { RequestsList } from "@/components/requests/requests-list";
import { RequestsListSkeleton } from "@/components/requests/requests-list-skeleton";
import { Button, buttonVariants } from "@/components/ui/button";
import { useMaintenanceRequests } from "@/hooks/use-maintenance-requests";
import { requestsPageSize, useRequestFilters } from "@/hooks/use-request-filters";
import { cn } from "@/lib/utils";

export function RequestsView() {
  const { filters, hasActiveFilters, setFilter, setPage, clearFilters } = useRequestFilters();
  const { data, error, isPending, isError, isPlaceholderData, refetch } = useMaintenanceRequests({
    ...filters,
    pageSize: requestsPageSize,
  });

  let content: ReactNode;

  if (isPending) {
    content = <RequestsListSkeleton />;
  } else if (isError) {
    content = (
      <ErrorState title="No se pudieron cargar las solicitudes" error={error} onRetry={() => refetch()} />
    );
  } else if (data.items.length === 0 && data.totalCount > 0) {
    content = (
      <EmptyState
        title="Esta página no tiene resultados"
        action={
          <Button variant="outline" onClick={() => setPage(1)}>
            Ir a la primera página
          </Button>
        }
      />
    );
  } else if (data.items.length === 0 && hasActiveFilters) {
    content = (
      <EmptyState
        title="No hay solicitudes que coincidan"
        description="Prueba con otros filtros o con otra búsqueda."
        action={
          <Button variant="outline" onClick={clearFilters}>
            Limpiar filtros
          </Button>
        }
      />
    );
  } else if (data.items.length === 0) {
    content = (
      <EmptyState
        title="Todavía no hay solicitudes"
        description="Cuando se registre una solicitud de mantenimiento, aparecerá aquí."
        action={
          <Link href="/requests/new" className={buttonVariants()}>
            Crear la primera solicitud
          </Link>
        }
      />
    );
  } else {
    content = (
      <div className={cn("space-y-4 transition-opacity", isPlaceholderData && "opacity-60")}>
        <RequestsList items={data.items} />
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          totalCount={data.totalCount}
          disabled={isPlaceholderData}
          onPageChange={setPage}
        />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <RequestsFilters
        filters={filters}
        hasActiveFilters={hasActiveFilters}
        onFilterChange={setFilter}
        onClear={clearFilters}
      />
      {content}
    </div>
  );
}
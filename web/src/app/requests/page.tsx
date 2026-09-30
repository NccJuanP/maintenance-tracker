import { Suspense } from "react";
import { PageHeader } from "@/components/common/page-header";
import { RequestsListSkeleton } from "@/components/requests/requests-list-skeleton";
import { RequestsView } from "@/components/requests/requests-view";

export default function RequestsPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Solicitudes"
        description="Consulta, filtra y gestiona las solicitudes de mantenimiento."
      />
      <Suspense fallback={<RequestsListSkeleton />}>
        <RequestsView />
      </Suspense>
    </div>
  );
}
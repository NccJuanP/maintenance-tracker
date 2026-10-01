import { PageHeader } from "@/components/common/page-header";
import { SummaryCards } from "@/components/dashboard/summary-cards";

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Panel de resumen"
        description="Estado general de las solicitudes de mantenimiento."
      />
      <SummaryCards />
    </div>
  );
}
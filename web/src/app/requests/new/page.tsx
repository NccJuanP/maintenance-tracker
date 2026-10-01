import { PageHeader } from "@/components/common/page-header";
import { NewRequestForm } from "@/components/requests/new-request-form";

export default function NewRequestPage() {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader
        title="Nueva solicitud"
        description="Describe la incidencia para que el equipo pueda atenderla."
      />
      <NewRequestForm />
    </div>
  );
}
import { PageHeader } from "@/components/common/page-header";

interface RequestDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function RequestDetailPage({ params }: RequestDetailPageProps) {
  const { id } = await params;

  return <PageHeader title="Detalle de la solicitud" description={`Solicitud ${id}`} />;
}
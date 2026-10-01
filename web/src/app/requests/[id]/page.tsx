import { RequestDetailView } from "@/components/requests/request-detail-view";

interface RequestDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function RequestDetailPage({ params }: RequestDetailPageProps) {
  const { id } = await params;

  return <RequestDetailView requestId={id} />;
}
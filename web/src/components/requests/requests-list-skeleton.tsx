import { Skeleton } from "@/components/ui/skeleton";

export function RequestsListSkeleton() {
  return (
    <div role="status" aria-label="Cargando solicitudes" className="space-y-2 rounded-xl border bg-card p-4">
      {Array.from({ length: 5 }, (_, index) => (
        <Skeleton key={index} className="h-10 w-full" />
      ))}
    </div>
  );
}
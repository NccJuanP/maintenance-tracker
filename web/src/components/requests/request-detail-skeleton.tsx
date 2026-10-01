import { Skeleton } from "@/components/ui/skeleton";

export function RequestDetailSkeleton() {
  return (
    <div role="status" aria-label="Cargando solicitud" className="space-y-6">
      <Skeleton className="h-7 w-40" />
      <div className="space-y-2">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-2/3" />
      </div>
      <div className="grid gap-6 lg:grid-cols-3">
        <Skeleton className="h-64 lg:order-last" />
        <div className="space-y-6 lg:col-span-2">
          <Skeleton className="h-32" />
          <Skeleton className="h-48" />
        </div>
      </div>
    </div>
  );
}
import { Badge } from "@/components/ui/badge";
import type { Priority, RequestStatus } from "@/lib/api/types";
import { priorityLabels, statusLabels } from "@/lib/labels";

const statusStyles: Record<RequestStatus, string> = {
  Pending: "border-amber-200 bg-amber-50 text-amber-800",
  InProgress: "border-blue-200 bg-blue-50 text-blue-800",
  OnHold: "border-violet-200 bg-violet-50 text-violet-800",
  Resolved: "border-emerald-200 bg-emerald-50 text-emerald-800",
  Cancelled: "border-zinc-200 bg-zinc-100 text-zinc-600",
};

const priorityStyles: Record<Priority, string> = {
  Low: "border-slate-200 bg-slate-50 text-slate-700",
  Medium: "border-sky-200 bg-sky-50 text-sky-800",
  High: "border-orange-200 bg-orange-50 text-orange-800",
  Critical: "border-red-200 bg-red-50 text-red-800",
};

export function StatusBadge({ status }: { status: RequestStatus }) {
  return (
    <Badge variant="outline" className={statusStyles[status]}>
      {statusLabels[status]}
    </Badge>
  );
}

export function PriorityBadge({ priority }: { priority: Priority }) {
  return (
    <Badge variant="outline" className={priorityStyles[priority]}>
      {priorityLabels[priority]}
    </Badge>
  );
}
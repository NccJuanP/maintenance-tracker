import type { Category, Priority, RequestStatus } from "@/lib/api/types";

export const statusLabels: Record<RequestStatus, string> = {
  Pending: "Pendiente",
  InProgress: "En progreso",
  OnHold: "En espera",
  Resolved: "Resuelta",
  Cancelled: "Cancelada",
};

export const priorityLabels: Record<Priority, string> = {
  Low: "Baja",
  Medium: "Media",
  High: "Alta",
  Critical: "Crítica",
};

export const categoryLabels: Record<Category, string> = {
  Infrastructure: "Infraestructura",
  Equipment: "Equipos",
  Software: "Software",
  Other: "Otros",
};

export const requestStatuses = Object.keys(statusLabels) as RequestStatus[];
export const priorities = Object.keys(priorityLabels) as Priority[];
export const categories = Object.keys(categoryLabels) as Category[];

export function transitionActionLabel(from: RequestStatus, to: RequestStatus): string {
  switch (to) {
    case "InProgress":
      return from === "OnHold" ? "Reanudar trabajo" : "Iniciar trabajo";
    case "OnHold":
      return "Poner en espera";
    case "Resolved":
      return "Marcar como resuelta";
    case "Cancelled":
      return "Cancelar solicitud";
    default:
      return statusLabels[to];
  }
}
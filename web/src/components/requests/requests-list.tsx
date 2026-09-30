import Link from "next/link";
import { PriorityBadge, StatusBadge } from "@/components/requests/request-badges";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { MaintenanceRequestListItem } from "@/lib/api/types";
import { formatDate, shortId } from "@/lib/format";
import { categoryLabels } from "@/lib/labels";

export function RequestsList({ items }: { items: MaintenanceRequestListItem[] }) {
  return (
    <>
      <div className="hidden rounded-xl border bg-card md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>ID</TableHead>
              <TableHead>Título</TableHead>
              <TableHead>Categoría</TableHead>
              <TableHead>Prioridad</TableHead>
              <TableHead>Estado</TableHead>
              <TableHead>Responsable</TableHead>
              <TableHead>Creada</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell className="font-mono text-xs text-muted-foreground" title={item.id}>
                  {shortId(item.id)}
                </TableCell>
                <TableCell className="max-w-xs truncate font-medium">
                  <Link href={`/requests/${item.id}`} className="hover:underline">
                    {item.title}
                  </Link>
                </TableCell>
                <TableCell>{categoryLabels[item.category]}</TableCell>
                <TableCell>
                  <PriorityBadge priority={item.priority} />
                </TableCell>
                <TableCell>
                  <StatusBadge status={item.status} />
                </TableCell>
                <TableCell>
                  {item.assignee?.name ?? <span className="text-muted-foreground">Sin asignar</span>}
                </TableCell>
                <TableCell>{formatDate(item.createdAt)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="grid gap-3 md:hidden">
        {items.map((item) => (
          <li key={item.id}>
            <Link
              href={`/requests/${item.id}`}
              className="block space-y-2 rounded-xl border bg-card p-4 transition-colors hover:bg-muted/50"
            >
              <div className="flex items-start justify-between gap-2">
                <p className="font-medium">{item.title}</p>
                <StatusBadge status={item.status} />
              </div>
              <div className="flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                <PriorityBadge priority={item.priority} />
                <span>{categoryLabels[item.category]}</span>
                <span>·</span>
                <span>{formatDate(item.createdAt)}</span>
              </div>
              <p className="text-xs text-muted-foreground">
                Responsable: {item.assignee?.name ?? "Sin asignar"}
              </p>
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
}
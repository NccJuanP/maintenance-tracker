import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";

interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  disabled?: boolean;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, totalPages, totalCount, disabled, onPageChange }: PaginationProps) {
  const resultsLabel = totalCount === 1 ? "1 resultado" : `${totalCount} resultados`;

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 text-sm">
      <p className="text-muted-foreground">
        Página {page} de {Math.max(totalPages, 1)} · {resultsLabel}
      </p>
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={disabled || page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          <ChevronLeft />
          Anterior
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={disabled || page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Siguiente
          <ChevronRight />
        </Button>
      </div>
    </div>
  );
}
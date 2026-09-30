"use client";

import type { FormEvent } from "react";
import { Search, X } from "lucide-react";
import { NativeSelect } from "@/components/common/native-select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import type { FilterKey, RequestFilters } from "@/hooks/use-request-filters";
import {
  categories,
  categoryLabels,
  priorities,
  priorityLabels,
  requestStatuses,
  statusLabels,
} from "@/lib/labels";

interface RequestsFiltersProps {
  filters: RequestFilters;
  hasActiveFilters: boolean;
  onFilterChange: (key: FilterKey, value: string | undefined) => void;
  onClear: () => void;
}

export function RequestsFilters({ filters, hasActiveFilters, onFilterChange, onClear }: RequestsFiltersProps) {
  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = new FormData(event.currentTarget).get("search");
    onFilterChange("search", typeof value === "string" ? value.trim() : undefined);
  }

  return (
    <div className="space-y-3 rounded-xl border bg-card p-4">
      <form onSubmit={handleSearch} className="flex gap-2">
        <Input
          key={filters.search ?? ""}
          name="search"
          defaultValue={filters.search}
          placeholder="Buscar por título"
          aria-label="Buscar por título"
          maxLength={120}
        />
        <Button type="submit" variant="outline">
          <Search />
          Buscar
        </Button>
      </form>

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <FilterSelect
          label="Estado"
          value={filters.status}
          options={requestStatuses}
          labels={statusLabels}
          onChange={(value) => onFilterChange("status", value)}
        />
        <FilterSelect
          label="Prioridad"
          value={filters.priority}
          options={priorities}
          labels={priorityLabels}
          onChange={(value) => onFilterChange("priority", value)}
        />
        <FilterSelect
          label="Categoría"
          value={filters.category}
          options={categories}
          labels={categoryLabels}
          onChange={(value) => onFilterChange("category", value)}
        />
        <label className="grid gap-1 text-sm">
          <span className="text-muted-foreground">Orden</span>
          <NativeSelect
            value={filters.sortDirection}
            onChange={(event) => onFilterChange("sortDirection", event.target.value)}
          >
            <option value="desc">Más recientes primero</option>
            <option value="asc">Más antiguas primero</option>
          </NativeSelect>
        </label>
      </div>

      {hasActiveFilters && (
        <Button variant="ghost" size="sm" onClick={onClear}>
          <X />
          Limpiar filtros
        </Button>
      )}
    </div>
  );
}

interface FilterSelectProps<T extends string> {
  label: string;
  value: T | undefined;
  options: readonly T[];
  labels: Record<T, string>;
  onChange: (value: T | undefined) => void;
}

function FilterSelect<T extends string>({ label, value, options, labels, onChange }: FilterSelectProps<T>) {
  return (
    <label className="grid gap-1 text-sm">
      <span className="text-muted-foreground">{label}</span>
      <NativeSelect
        value={value ?? ""}
        onChange={(event) => onChange((event.target.value || undefined) as T | undefined)}
      >
        <option value="">Todos</option>
        {options.map((option) => (
          <option key={option} value={option}>
            {labels[option]}
          </option>
        ))}
      </NativeSelect>
    </label>
  );
}
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { Category, Priority, RequestStatus } from "@/lib/api/types";
import { categories, priorities, requestStatuses } from "@/lib/labels";

export const requestsPageSize = 10;

export type SortDirection = "asc" | "desc";

export type FilterKey = "status" | "priority" | "category" | "search" | "sortDirection";

export interface RequestFilters {
  status?: RequestStatus;
  priority?: Priority;
  category?: Category;
  search?: string;
  sortDirection: SortDirection;
  page: number;
}

function pickAllowed<T extends string>(value: string | null, allowed: readonly T[]): T | undefined {
  return allowed.find((option) => option === value);
}

function parsePage(value: string | null): number {
  const page = Number(value);
  return Number.isInteger(page) && page >= 1 ? page : 1;
}

export function useRequestFilters() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const filters: RequestFilters = {
    status: pickAllowed(searchParams.get("status"), requestStatuses),
    priority: pickAllowed(searchParams.get("priority"), priorities),
    category: pickAllowed(searchParams.get("category"), categories),
    search: searchParams.get("search")?.trim() || undefined,
    sortDirection: searchParams.get("sortDirection") === "asc" ? "asc" : "desc",
    page: parsePage(searchParams.get("page")),
  };

  const hasActiveFilters = Boolean(filters.status || filters.priority || filters.category || filters.search);

  function navigate(params: URLSearchParams) {
    const queryString = params.toString();
    router.replace(queryString ? `${pathname}?${queryString}` : pathname, { scroll: false });
  }

  function setFilter(key: FilterKey, value: string | undefined) {
    const params = new URLSearchParams(searchParams.toString());
    if (value) {
      params.set(key, value);
    } else {
      params.delete(key);
    }
    params.delete("page");
    navigate(params);
  }

  function setPage(page: number) {
    const params = new URLSearchParams(searchParams.toString());
    if (page > 1) {
      params.set("page", String(page));
    } else {
      params.delete("page");
    }
    navigate(params);
  }

  function clearFilters() {
    const params = new URLSearchParams();
    if (filters.sortDirection === "asc") {
      params.set("sortDirection", "asc");
    }
    navigate(params);
  }

  return { filters, hasActiveFilters, setFilter, setPage, clearFilters };
}
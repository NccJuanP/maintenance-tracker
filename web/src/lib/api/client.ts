import type { ApiProblem } from "./types";

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly problem: ApiProblem;

  constructor(problem: ApiProblem) {
    super(problem.detail ?? problem.title);
    this.name = "ApiError";
    this.status = problem.status;
    this.code = problem.code;
    this.problem = problem;
  }
}

type QueryValue = string | number | undefined;

interface RequestOptions {
  method?: "GET" | "POST";
  query?: Record<string, QueryValue>;
  body?: unknown;
  userId?: string;
  signal?: AbortSignal;
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", query, body, userId, signal } = options;

  const headers: Record<string, string> = { Accept: "application/json" };
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }
  if (userId) {
    headers["X-User-Id"] = userId;
  }

  let response: Response;
  try {
    response = await fetch(buildUrl(path, query), {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    });
  } catch (error) {
    if (signal?.aborted) {
      throw error;
    }
    throw new ApiError({
      title: "No se pudo conectar con el servidor.",
      status: 0,
      code: "network_error",
      detail: "Verifica que la API esté corriendo e intenta de nuevo.",
    });
  }

  if (!response.ok) {
    throw new ApiError(await readProblem(response));
  }

  return (await response.json()) as T;
}

function buildUrl(path: string, query: Record<string, QueryValue> = {}): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_URL;
  if (!baseUrl) {
    throw new Error("NEXT_PUBLIC_API_URL is not set. Check web/.env.local.");
  }

  const url = new URL(path, baseUrl);
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== "") {
      url.searchParams.set(key, String(value));
    }
  }
  return url.toString();
}

async function readProblem(response: Response): Promise<ApiProblem> {
  try {
    const problem = (await response.json()) as ApiProblem;
    if (problem.code) {
      return problem;
    }
  } catch {
    // The body was not JSON; fall back to the generic error below.
  }

  return {
    title: "Ocurrió un error inesperado.",
    status: response.status,
    code: "unexpected_error",
  };
}
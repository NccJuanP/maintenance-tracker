import type { Category, CreateMaintenanceRequestBody, Priority } from "@/lib/api/types";

export const titleLimits = { min: 5, max: 120 };
export const descriptionLimits = { min: 10, max: 2000 };

export interface NewRequestFormValues {
  title: string;
  description: string;
  category: Category | "";
  priority: Priority | "";
}

export type NewRequestField = keyof NewRequestFormValues;
export type NewRequestErrors = Partial<Record<NewRequestField, string>>;

export const emptyNewRequest: NewRequestFormValues = {
  title: "",
  description: "",
  category: "",
  priority: "",
};

type ParseResult =
  | { ok: true; body: CreateMaintenanceRequestBody }
  | { ok: false; errors: NewRequestErrors };

export function parseNewRequest(values: NewRequestFormValues): ParseResult {
  const title = values.title.trim();
  const description = values.description.trim();
  const errors: NewRequestErrors = {};

  if (title.length < titleLimits.min || title.length > titleLimits.max) {
    errors.title = `El título debe tener entre ${titleLimits.min} y ${titleLimits.max} caracteres.`;
  }

  if (description.length < descriptionLimits.min || description.length > descriptionLimits.max) {
    errors.description = `La descripción debe tener entre ${descriptionLimits.min} y ${descriptionLimits.max} caracteres.`;
  }

  if (!values.category) {
    errors.category = "Selecciona una categoría.";
  }

  if (!values.priority) {
    errors.priority = "Selecciona una prioridad.";
  }

  if (!values.category || !values.priority || Object.keys(errors).length > 0) {
    return { ok: false, errors };
  }

  return {
    ok: true,
    body: { title, description, category: values.category, priority: values.priority },
  };
}

const formFields: NewRequestField[] = ["title", "description", "category", "priority"];

export function errorsFromProblem(problemErrors: Record<string, string[]> | undefined): NewRequestErrors {
  const errors: NewRequestErrors = {};

  for (const field of formFields) {
    const message = problemErrors?.[field]?.[0];
    if (message) {
      errors[field] = message;
    }
  }

  return errors;
}
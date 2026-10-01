"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { type FormEvent, useState } from "react";
import { toast } from "sonner";
import { FormField } from "@/components/common/form-field";
import { NativeSelect } from "@/components/common/native-select";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCurrentUser } from "@/hooks/use-current-user";
import { useCreateRequest } from "@/hooks/use-request-mutations";
import { ApiError } from "@/lib/api/client";
import type { Category, Priority } from "@/lib/api/types";
import { categories, categoryLabels, priorities, priorityLabels } from "@/lib/labels";
import {
  descriptionLimits,
  emptyNewRequest,
  errorsFromProblem,
  parseNewRequest,
  titleLimits,
  type NewRequestErrors,
  type NewRequestFormValues,
} from "@/lib/request-form";

export function NewRequestForm() {
  const router = useRouter();
  const { user } = useCurrentUser();
  const create = useCreateRequest();
  const [values, setValues] = useState<NewRequestFormValues>(emptyNewRequest);
  const [errors, setErrors] = useState<NewRequestErrors>({});

  function updateField<K extends keyof NewRequestFormValues>(field: K, value: NewRequestFormValues[K]) {
    setValues((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!user) {
      return;
    }

    const result = parseNewRequest(values);
    if (!result.ok) {
      setErrors(result.errors);
      return;
    }

    create.mutate(
      { body: result.body, actorId: user.id },
      {
        onSuccess: (created) => {
          toast.success("Solicitud creada.");
          router.replace(`/requests/${created.id}`);
        },
        onError: (error) => {
          if (error instanceof ApiError && error.code === "validation_failed") {
            const serverErrors = errorsFromProblem(error.problem.errors);
            if (Object.keys(serverErrors).length > 0) {
              setErrors(serverErrors);
              return;
            }
          }
          toast.error(error instanceof ApiError ? error.message : "Ocurrió un error inesperado.");
        },
      },
    );
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card>
        <CardContent className="grid gap-5">
          <FormField
            id="title"
            label="Título"
            error={errors.title}
            hint={`${values.title.length}/${titleLimits.max}`}
          >
            <Input
              id="title"
              value={values.title}
              onChange={(event) => updateField("title", event.target.value)}
              maxLength={titleLimits.max}
              placeholder="Ej.: Filtración de agua en la bodega 2"
              aria-invalid={Boolean(errors.title)}
              aria-describedby={errors.title ? "title-error" : undefined}
            />
          </FormField>

          <FormField
            id="description"
            label="Descripción"
            error={errors.description}
            hint={`${values.description.length}/${descriptionLimits.max}`}
          >
            <Textarea
              id="description"
              className="min-h-32"
              value={values.description}
              onChange={(event) => updateField("description", event.target.value)}
              maxLength={descriptionLimits.max}
              placeholder="Qué ocurre, dónde y desde cuándo."
              aria-invalid={Boolean(errors.description)}
              aria-describedby={errors.description ? "description-error" : undefined}
            />
          </FormField>

          <div className="grid gap-5 sm:grid-cols-2">
            <FormField id="category" label="Categoría" error={errors.category}>
              <NativeSelect
                id="category"
                value={values.category}
                onChange={(event) => updateField("category", event.target.value as Category | "")}
                aria-invalid={Boolean(errors.category)}
                aria-describedby={errors.category ? "category-error" : undefined}
              >
                <option value="" disabled>
                  Selecciona una categoría
                </option>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {categoryLabels[category]}
                  </option>
                ))}
              </NativeSelect>
            </FormField>

            <FormField id="priority" label="Prioridad" error={errors.priority}>
              <NativeSelect
                id="priority"
                value={values.priority}
                onChange={(event) => updateField("priority", event.target.value as Priority | "")}
                aria-invalid={Boolean(errors.priority)}
                aria-describedby={errors.priority ? "priority-error" : undefined}
              >
                <option value="" disabled>
                  Selecciona una prioridad
                </option>
                {priorities.map((priority) => (
                  <option key={priority} value={priority}>
                    {priorityLabels[priority]}
                  </option>
                ))}
              </NativeSelect>
            </FormField>
          </div>

          <p className="text-sm text-muted-foreground">
            {user ? (
              <>
                Se registrará a nombre de <strong>{user.name}</strong> y quedará en estado Pendiente.
              </>
            ) : (
              "Selecciona un usuario en la parte superior para registrar la solicitud."
            )}
          </p>
        </CardContent>

        <CardFooter className="justify-end gap-2">
          <Link href="/requests" className={buttonVariants({ variant: "outline" })}>
            Cancelar
          </Link>
          <Button type="submit" disabled={!user || create.isPending}>
            {create.isPending ? "Creando..." : "Crear solicitud"}
          </Button>
        </CardFooter>
      </Card>
    </form>
  );
}
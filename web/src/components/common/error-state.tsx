import { CircleAlert, RotateCw } from "lucide-react";
import { Alert, AlertAction, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { ApiError } from "@/lib/api/client";

interface ErrorStateProps {
  title?: string;
  error: unknown;
  onRetry?: () => void;
}

export function ErrorState({ title = "No se pudo cargar la información", error, onRetry }: ErrorStateProps) {
  const message = error instanceof ApiError ? error.message : "Ocurrió un error inesperado.";

  return (
    <Alert variant="destructive">
      <CircleAlert />
      <AlertTitle>{title}</AlertTitle>
      <AlertDescription>{message}</AlertDescription>
      {onRetry && (
        <AlertAction>
          <Button size="sm" variant="outline" onClick={onRetry}>
            <RotateCw />
            Reintentar
          </Button>
        </AlertAction>
      )}
    </Alert>
  );
}
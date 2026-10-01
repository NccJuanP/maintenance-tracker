import type { ReactNode } from "react";
import { Label } from "@/components/ui/label";

interface FormFieldProps {
  id: string;
  label: string;
  error?: string;
  hint?: ReactNode;
  children: ReactNode;
}

export function FormField({ id, label, error, hint, children }: FormFieldProps) {
  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>{label}</Label>
      {children}
      {(error || hint) && (
        <div className="flex items-start justify-between gap-3 text-xs">
          <p id={`${id}-error`} className="text-destructive">
            {error}
          </p>
          {hint && <span className="shrink-0 text-muted-foreground">{hint}</span>}
        </div>
      )}
    </div>
  );
}
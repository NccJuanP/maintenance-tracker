"use client";

import { useState } from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { useCurrentUser } from "@/hooks/use-current-user";
import { useChangeRequestStatus } from "@/hooks/use-request-mutations";
import type { MaintenanceRequestDetail, RequestStatus } from "@/lib/api/types";
import { statusLabels, transitionActionLabel } from "@/lib/labels";

const finalStatuses: RequestStatus[] = ["Resolved", "Cancelled"];

function buttonVariantFor(target: RequestStatus) {
  if (target === "Cancelled") {
    return "destructive";
  }
  return target === "Resolved" ? "default" : "outline";
}

export function StatusActions({ request }: { request: MaintenanceRequestDetail }) {
  const { user } = useCurrentUser();
  const changeStatus = useChangeRequestStatus(request.id);
  const [confirmTarget, setConfirmTarget] = useState<RequestStatus | null>(null);

  function submit(targetStatus: RequestStatus) {
    if (!user) {
      return;
    }
    changeStatus.mutate({ targetStatus, actorId: user.id });
  }

  function handleClick(target: RequestStatus) {
    if (finalStatuses.includes(target)) {
      setConfirmTarget(target);
    } else {
      submit(target);
    }
  }

  function handleConfirm() {
    if (confirmTarget) {
      submit(confirmTarget);
    }
    setConfirmTarget(null);
  }

  if (request.allowedTransitions.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        Esta solicitud está {statusLabels[request.status].toLowerCase()} y ya no admite cambios.
      </p>
    );
  }

  return (
    <div className="grid gap-2">
      <p className="text-sm font-medium">Estado</p>
      <div className="flex flex-wrap gap-2">
        {request.allowedTransitions.map((target) => (
          <Button
            key={target}
            variant={buttonVariantFor(target)}
            disabled={!user || changeStatus.isPending}
            onClick={() => handleClick(target)}
          >
            {transitionActionLabel(request.status, target)}
          </Button>
        ))}
      </div>
      {!request.assignee && (
        <p className="text-xs text-muted-foreground">Asigna un responsable para poder iniciar el trabajo.</p>
      )}

      <AlertDialog
        open={confirmTarget !== null}
        onOpenChange={(open) => {
          if (!open) {
            setConfirmTarget(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {confirmTarget && transitionActionLabel(request.status, confirmTarget)}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {confirmTarget &&
                `La solicitud quedará ${statusLabels[confirmTarget].toLowerCase()} y ya no admitirá cambios de estado ni de responsable.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Volver</AlertDialogCancel>
            <AlertDialogAction
              variant={confirmTarget === "Cancelled" ? "destructive" : "default"}
              onClick={handleConfirm}
            >
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
import { useSyncExternalStore } from "react";

const storageKey = "maintenance-tracker.user-id";
const listeners = new Set<() => void>();

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  window.addEventListener("storage", listener);
  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", listener);
  };
}

function getSnapshot(): string | null {
  return window.localStorage.getItem(storageKey);
}

function getServerSnapshot(): string | null {
  return null;
}

export function useStoredUserId(): string | null {
  return useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);
}

export function setStoredUserId(userId: string): void {
  window.localStorage.setItem(storageKey, userId);
  listeners.forEach((listener) => listener());
}
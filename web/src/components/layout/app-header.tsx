"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { HardHat, Plus } from "lucide-react";
import { UserSelect } from "@/components/layout/user-select";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const navItems = [
  { href: "/", label: "Panel" },
  { href: "/requests", label: "Solicitudes" },
];

function isActive(pathname: string, href: string): boolean {
  return href === "/" ? pathname === "/" : pathname.startsWith(href);
}

export function AppHeader() {
  const pathname = usePathname();

  return (
    <header className="border-b bg-background">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-6 gap-y-3 px-4 py-3">
        <Link href="/" className="flex items-center gap-2 font-semibold">
          <HardHat className="size-5" />
          Centro de mantenimiento
        </Link>

        <nav className="flex gap-1">
          {navItems.map((item) => {
            const active = isActive(pathname, item.href);
            return (
              <Link
                key={item.href}
                href={item.href}
                aria-current={active ? "page" : undefined}
                className={cn(buttonVariants({ variant: "ghost" }), active && "bg-muted text-foreground")}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="ml-auto flex flex-wrap items-center gap-3">
          <UserSelect />
          <Link href="/requests/new" className={buttonVariants()}>
            <Plus />
            Nueva solicitud
          </Link>
        </div>
      </div>
    </header>
  );
}
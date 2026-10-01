import type { Metadata } from "next";
import { Geist_Mono, Inter } from "next/font/google";
import { AppHeader } from "@/components/layout/app-header";
import { Providers } from "./providers";
import "./globals.css";

const inter = Inter({
  variable: "--font-sans",
  subsets: ["latin"],
});


const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Centro de mantenimiento",
  description: "Gestión de solicitudes de mantenimiento de instalaciones y equipos.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="es" className={`${inter.variable} ${geistMono.variable} h-full antialiased`}>
      <body className="flex min-h-full flex-col bg-muted/40">
        <Providers>
          <AppHeader />
          <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6">{children}</main>
        </Providers>
      </body>
    </html>
  );
}
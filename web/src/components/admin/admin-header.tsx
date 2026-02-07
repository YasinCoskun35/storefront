"use client";

import { useAuth } from "@/lib/auth-context";
import { Button } from "@/components/ui/button";
import { LogOut, User } from "lucide-react";

export function AdminHeader() {
  const { user, logout } = useAuth();

  return (
    <header className="flex h-16 items-center border-b bg-background px-6">
      <div className="flex flex-1 items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold">Welcome back, {user?.firstName}</h2>
          <p className="text-sm text-muted-foreground">{user?.email}</p>
        </div>
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-2 text-sm">
            <User className="h-4 w-4" />
            <span className="font-medium">{user?.roles?.join(", ")}</span>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={logout}
            className="flex items-center space-x-2"
          >
            <LogOut className="h-4 w-4" />
            <span>Logout</span>
          </Button>
        </div>
      </div>
    </header>
  );
}


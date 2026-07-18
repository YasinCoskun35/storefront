"use client";

import { createContext, useContext, useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { api, authApi } from "@/lib/api";

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    // Check if user is logged in (from localStorage)
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    try {
      // Call backend directly — httpOnly cookie is set on the browser response
      const data = await authApi.login(email, password);

      // Store only user metadata for UI state; token is in the httpOnly cookie
      localStorage.setItem("user", JSON.stringify(data.user));

      setUser(data.user);
      router.push("/admin/dashboard");
    } catch (error: any) {
      throw error;
    }
  };

  const logout = async () => {
    try {
      await api.post("/api/identity/auth/logout");
    } catch {
      // Proceed with client-side cleanup even if the backend call fails
    }
    localStorage.removeItem("user");
    setUser(null);
    router.push("/login");
  };

  const isAdmin = user?.roles?.includes("Admin") || false;

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading, isAdmin }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}


"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Loader2, Search, Users } from "lucide-react";

interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
  createdAt: string;
}

async function fetchUsers(search?: string): Promise<AdminUser[]> {
  const response = await api.get("/api/identity/admin/users", {
    params: { search },
  });
  return response.data;
}

export default function AdminUsersPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  const { data: users, isLoading } = useQuery({
    queryKey: ["admin-users", debouncedSearch],
    queryFn: () => fetchUsers(debouncedSearch || undefined),
  });

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    // Simple debounce
    clearTimeout((window as any)._userSearchTimer);
    (window as any)._userSearchTimer = setTimeout(() => {
      setDebouncedSearch(e.target.value);
    }, 300);
  };

  const roleColor = (role: string) => {
    switch (role) {
      case "Admin": return "bg-red-100 text-red-800";
      case "Manager": return "bg-blue-100 text-blue-800";
      default: return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary flex items-center gap-2">
            <Users className="h-8 w-8" />
            Admin Users
          </h1>
          <p className="text-muted-foreground">Manage admin users and roles</p>
        </div>
      </div>

      {/* Search */}
      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search by name or email..."
          value={search}
          onChange={handleSearch}
          className="pl-9"
        />
      </div>

      {/* Table */}
      <div className="rounded-md border bg-card">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-4 py-3 text-left text-sm font-medium">Name</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Email</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Roles</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Joined</th>
              </tr>
            </thead>
            <tbody>
              {users?.map((user) => (
                <tr key={user.id} className="border-b last:border-0 hover:bg-muted/50">
                  <td className="px-4 py-3 text-sm font-medium">
                    {user.firstName} {user.lastName}
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">
                    {user.email}
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <div className="flex gap-1 flex-wrap">
                      {user.roles.map((role) => (
                        <span
                          key={role}
                          className={`px-2 py-0.5 rounded text-xs font-medium ${roleColor(role)}`}
                        >
                          {role}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <Badge variant={user.isActive ? "default" : "secondary"}>
                      {user.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">
                    {new Date(user.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
              {users?.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-12 text-center text-muted-foreground">
                    No users found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <p className="text-sm text-muted-foreground">
        Total: {users?.length ?? 0} user{users?.length !== 1 ? "s" : ""}
      </p>
    </div>
  );
}

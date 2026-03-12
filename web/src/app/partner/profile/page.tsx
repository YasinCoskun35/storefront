"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  ArrowLeft,
  Building2,
  User,
  Mail,
  Phone,
  MapPin,
  ShieldCheck,
  KeyRound,
  Loader2,
} from "lucide-react";
import Link from "next/link";
import { toast } from "sonner";
import { api } from "@/lib/api";

interface PartnerProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  jobTitle?: string;
  phoneNumber?: string;
  isAdmin: boolean;
  company: {
    id: string;
    name: string;
    taxNumber?: string;
    email?: string;
    phone?: string;
    address?: string;
    city?: string;
    country?: string;
    status: string;
  };
}

export default function PartnerProfilePage() {
  const router = useRouter();
  const [profile, setProfile] = useState<PartnerProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [changingPassword, setChangingPassword] = useState(false);
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  const [savingPassword, setSavingPassword] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("partner_access_token");
    if (!token) {
      router.push("/partner/login");
      return;
    }

    const fetchProfile = async () => {
      try {
        const res = await api.get<PartnerProfile>("/api/identity/partners/profile");
        setProfile(res.data);
      } catch {
        toast.error("Failed to load profile");
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [router]);

  const handlePasswordChange = async () => {
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      toast.error("Passwords do not match");
      return;
    }
    if (passwordForm.newPassword.length < 8) {
      toast.error("Password must be at least 8 characters");
      return;
    }
    setSavingPassword(true);
    try {
      await api.post("/api/identity/partner/change-password", {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      });
      toast.success("Password changed successfully");
      setChangingPassword(false);
      setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
    } catch (err: any) {
      toast.error(err.response?.data?.message || "Failed to change password");
    } finally {
      setSavingPassword(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!profile) return null;

  return (
    <div className="container mx-auto px-4 py-8 max-w-3xl">
      <div className="mb-6">
        <Link href="/partner/dashboard">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>
        </Link>
      </div>

      <h1 className="text-3xl font-bold mb-6">My Profile</h1>

      <div className="space-y-6">
        {/* Personal Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Personal Information
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <Label className="text-xs text-muted-foreground">First Name</Label>
              <p className="font-medium">{profile.firstName}</p>
            </div>
            <div>
              <Label className="text-xs text-muted-foreground">Last Name</Label>
              <p className="font-medium">{profile.lastName}</p>
            </div>
            <div>
              <Label className="text-xs text-muted-foreground flex items-center gap-1">
                <Mail className="h-3 w-3" /> Email
              </Label>
              <p className="font-medium">{profile.email}</p>
            </div>
            {profile.phoneNumber && (
              <div>
                <Label className="text-xs text-muted-foreground flex items-center gap-1">
                  <Phone className="h-3 w-3" /> Phone
                </Label>
                <p className="font-medium">{profile.phoneNumber}</p>
              </div>
            )}
            {profile.jobTitle && (
              <div>
                <Label className="text-xs text-muted-foreground">Job Title</Label>
                <p className="font-medium">{profile.jobTitle}</p>
              </div>
            )}
            <div>
              <Label className="text-xs text-muted-foreground flex items-center gap-1">
                <ShieldCheck className="h-3 w-3" /> Role
              </Label>
              <Badge variant={profile.isAdmin ? "default" : "secondary"}>
                {profile.isAdmin ? "Company Admin" : "Member"}
              </Badge>
            </div>
          </CardContent>
        </Card>

        {/* Company Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Company Information
            </CardTitle>
            <CardDescription>Your company details managed by the administrator</CardDescription>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2">
              <Label className="text-xs text-muted-foreground">Company Name</Label>
              <p className="font-medium text-lg">{profile.company.name}</p>
            </div>
            {profile.company.taxNumber && (
              <div>
                <Label className="text-xs text-muted-foreground">Tax Number</Label>
                <p className="font-medium">{profile.company.taxNumber}</p>
              </div>
            )}
            <div>
              <Label className="text-xs text-muted-foreground">Status</Label>
              <Badge
                variant={
                  profile.company.status === "Active"
                    ? "default"
                    : profile.company.status === "Suspended"
                    ? "destructive"
                    : "secondary"
                }
              >
                {profile.company.status}
              </Badge>
            </div>
            {profile.company.email && (
              <div>
                <Label className="text-xs text-muted-foreground flex items-center gap-1">
                  <Mail className="h-3 w-3" /> Company Email
                </Label>
                <p className="font-medium">{profile.company.email}</p>
              </div>
            )}
            {profile.company.phone && (
              <div>
                <Label className="text-xs text-muted-foreground flex items-center gap-1">
                  <Phone className="h-3 w-3" /> Company Phone
                </Label>
                <p className="font-medium">{profile.company.phone}</p>
              </div>
            )}
            {(profile.company.address || profile.company.city || profile.company.country) && (
              <div className="sm:col-span-2">
                <Label className="text-xs text-muted-foreground flex items-center gap-1">
                  <MapPin className="h-3 w-3" /> Address
                </Label>
                <p className="font-medium">
                  {[profile.company.address, profile.company.city, profile.company.country]
                    .filter(Boolean)
                    .join(", ")}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Change Password */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <KeyRound className="h-5 w-5" />
                Security
              </CardTitle>
              {!changingPassword && (
                <Button variant="outline" size="sm" onClick={() => setChangingPassword(true)}>
                  Change Password
                </Button>
              )}
            </div>
          </CardHeader>
          {changingPassword && (
            <CardContent className="space-y-3">
              <div>
                <Label>Current Password</Label>
                <Input
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(e) =>
                    setPasswordForm({ ...passwordForm, currentPassword: e.target.value })
                  }
                />
              </div>
              <div>
                <Label>New Password</Label>
                <Input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(e) =>
                    setPasswordForm({ ...passwordForm, newPassword: e.target.value })
                  }
                />
              </div>
              <div>
                <Label>Confirm New Password</Label>
                <Input
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(e) =>
                    setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })
                  }
                />
              </div>
              <div className="flex gap-2 pt-2">
                <Button
                  variant="outline"
                  onClick={() => {
                    setChangingPassword(false);
                    setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
                  }}
                >
                  Cancel
                </Button>
                <Button disabled={savingPassword} onClick={handlePasswordChange}>
                  {savingPassword && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Update Password
                </Button>
              </div>
            </CardContent>
          )}
        </Card>
      </div>
    </div>
  );
}

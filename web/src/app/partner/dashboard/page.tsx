'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Building2,
  Package,
  ShoppingCart,
  Users,
  FileText,
  LogOut,
  Settings,
  Bell,
} from 'lucide-react';

interface PartnerUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  company: {
    id: string;
    name: string;
    status: string;
  };
}

export default function PartnerDashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<PartnerUser | null>(null);

  useEffect(() => {
    // Load user from localStorage
    const userStr = localStorage.getItem('partner_user');
    if (!userStr) {
      router.push('/partner/login');
      return;
    }
    setUser(JSON.parse(userStr));
  }, [router]);

  const handleLogout = () => {
    localStorage.removeItem('partner_access_token');
    localStorage.removeItem('partner_refresh_token');
    localStorage.removeItem('partner_user');
    router.push('/partner/login');
  };

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Building2 className="h-8 w-8 text-blue-600" />
              <div>
                <h1 className="text-xl font-bold">{user.company.name}</h1>
                <p className="text-sm text-gray-600">Partner Portal</p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <Button variant="ghost" size="sm">
                <Bell className="h-5 w-5" />
              </Button>
              <Link href="/partner/profile">
                <Button variant="ghost" size="sm">
                  <Settings className="h-5 w-5 mr-2" />
                  Settings
                </Button>
              </Link>
              <Button variant="ghost" size="sm" onClick={handleLogout}>
                <LogOut className="h-5 w-5 mr-2" />
                Logout
              </Button>
            </div>
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-8">
        {/* Welcome */}
        <div className="mb-8">
          <h2 className="text-3xl font-bold">
            Welcome back, {user.firstName}!
          </h2>
          <p className="text-gray-600 mt-1">
            {user.role === 'CompanyAdmin' ? 'Company Administrator' : 'User'} • {user.email}
          </p>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Pending Orders</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">0</div>
              <p className="text-sm text-gray-500 mt-1">Awaiting quotes</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Active Orders</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">0</div>
              <p className="text-sm text-gray-500 mt-1">In progress</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Total Orders</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">0</div>
              <p className="text-sm text-gray-500 mt-1">All time</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Company Status</CardTitle>
            </CardHeader>
            <CardContent>
              <Badge className="bg-green-100 text-green-800">{user.company.status}</Badge>
              <p className="text-sm text-gray-500 mt-2">Account verified</p>
            </CardContent>
          </Card>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardHeader>
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mb-2">
                <Package className="h-6 w-6 text-blue-600" />
              </div>
              <CardTitle>Browse Catalog</CardTitle>
              <CardDescription>View our complete furniture collection</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/products">
                <Button variant="outline" className="w-full">
                  View Products
                </Button>
              </Link>
            </CardContent>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardHeader>
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mb-2">
                <ShoppingCart className="h-6 w-6 text-green-600" />
              </div>
              <CardTitle>Request Quote</CardTitle>
              <CardDescription>Submit a new order request</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/partner/orders/new">
                <Button className="w-full">New Request</Button>
              </Link>
            </CardContent>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardHeader>
              <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center mb-2">
                <FileText className="h-6 w-6 text-purple-600" />
              </div>
              <CardTitle>Order History</CardTitle>
              <CardDescription>View past orders and quotes</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/partner/orders">
                <Button variant="outline" className="w-full">
                  View Orders
                </Button>
              </Link>
            </CardContent>
          </Card>
        </div>

        {/* Recent Activity */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Recent Orders</CardTitle>
              <CardDescription>Your latest order requests</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8 text-gray-500">
                <Package className="h-12 w-12 mx-auto mb-3 opacity-20" />
                <p>No orders yet</p>
                <p className="text-sm mt-1">Start by requesting a quote</p>
              </div>
            </CardContent>
          </Card>

          {user.role === 'CompanyAdmin' && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center">
                  <Users className="h-5 w-5 mr-2" />
                  Company Users
                </CardTitle>
                <CardDescription>Manage your team members</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <div className="flex items-center justify-between p-3 bg-gray-50 rounded">
                    <div>
                      <div className="font-medium">
                        {user.firstName} {user.lastName}
                      </div>
                      <div className="text-sm text-gray-600">{user.email}</div>
                    </div>
                    <Badge>{user.role}</Badge>
                  </div>
                </div>
                <Link href="/partner/users">
                  <Button variant="outline" className="w-full mt-4">
                    Manage Users
                  </Button>
                </Link>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

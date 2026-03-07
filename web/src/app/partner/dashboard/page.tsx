'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
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
  Clock,
  CheckCircle,
  ArrowRight,
} from 'lucide-react';
import { partnerOrdersApi } from '@/lib/api/orders';

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
  const [tokenLoaded, setTokenLoaded] = useState(false);

  useEffect(() => {
    const userStr = localStorage.getItem('partner_user');
    if (!userStr) {
      router.push('/partner/login');
      return;
    }
    setUser(JSON.parse(userStr));
    setTokenLoaded(true);
  }, [router]);

  const { data: stats } = useQuery({
    queryKey: ['partner-order-stats'],
    queryFn: () => partnerOrdersApi.getStats(),
    enabled: tokenLoaded,
  });

  const { data: recentOrders } = useQuery({
    queryKey: ['partner-recent-orders'],
    queryFn: () => partnerOrdersApi.getOrders({ pageNumber: 1, pageSize: 5 }),
    enabled: tokenLoaded,
  });

  const handleLogout = () => {
    localStorage.removeItem('partner_access_token');
    localStorage.removeItem('partner_refresh_token');
    localStorage.removeItem('partner_user');
    localStorage.removeItem('accessToken');
    router.push('/partner/login');
  };

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900" />
      </div>
    );
  }

  const statusBadge = (status: string) => {
    const colors: Record<string, string> = {
      Pending: 'bg-yellow-100 text-yellow-800',
      QuoteSent: 'bg-blue-100 text-blue-800',
      Confirmed: 'bg-green-100 text-green-800',
      Preparing: 'bg-purple-100 text-purple-800',
      Shipping: 'bg-cyan-100 text-cyan-800',
      Delivered: 'bg-green-100 text-green-800',
      Cancelled: 'bg-red-100 text-red-800',
    };
    return colors[status] ?? 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Building2 className="h-8 w-8 text-blue-600" />
              <div>
                <h1 className="text-xl font-bold">{user.company.name}</h1>
                <p className="text-sm text-gray-600">Partner Portal</p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <Link href="/partner/cart">
                <Button variant="ghost" size="sm">
                  <ShoppingCart className="h-5 w-5 mr-2" />
                  Cart
                </Button>
              </Link>
              <Link href="/partner/profile">
                <Button variant="ghost" size="sm">
                  <Users className="h-5 w-5 mr-2" />
                  Profile
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
          <h2 className="text-3xl font-bold text-gray-900">
            Welcome back, {user.firstName}!
          </h2>
          <p className="text-gray-600 mt-1">
            {user.role === 'CompanyAdmin' ? 'Company Administrator' : 'User'} •{' '}
            <span className="font-medium">{user.company.name}</span>
          </p>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          {[
            {
              label: 'Total Orders',
              value: stats?.totalOrders ?? 0,
              icon: FileText,
              color: 'text-purple-600',
              bg: 'bg-purple-50',
            },
            {
              label: 'Pending Quote',
              value: stats?.pendingOrders ?? 0,
              icon: Clock,
              color: 'text-yellow-600',
              bg: 'bg-yellow-50',
            },
            {
              label: 'Active Orders',
              value: stats?.activeOrders ?? 0,
              icon: Package,
              color: 'text-blue-600',
              bg: 'bg-blue-50',
            },
            {
              label: 'Completed',
              value: stats?.completedOrders ?? 0,
              icon: CheckCircle,
              color: 'text-green-600',
              bg: 'bg-green-50',
            },
          ].map((s) => {
            const Icon = s.icon;
            return (
              <Card key={s.label}>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-lg ${s.bg}`}>
                      <Icon className={`h-5 w-5 ${s.color}`} />
                    </div>
                    <div>
                      <div className="text-2xl font-bold">{s.value}</div>
                      <div className="text-xs text-gray-500">{s.label}</div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <Card className="hover:shadow-md transition-shadow">
            <CardHeader>
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center mb-2">
                <Package className="h-5 w-5 text-blue-600" />
              </div>
              <CardTitle className="text-base">Browse Catalog</CardTitle>
              <CardDescription>View our complete collection</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/products">
                <Button variant="outline" className="w-full">
                  View Products
                </Button>
              </Link>
            </CardContent>
          </Card>

          <Card className="hover:shadow-md transition-shadow">
            <CardHeader>
              <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mb-2">
                <ShoppingCart className="h-5 w-5 text-green-600" />
              </div>
              <CardTitle className="text-base">My Cart</CardTitle>
              <CardDescription>Review and submit your cart</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/partner/cart">
                <Button className="w-full">Go to Cart</Button>
              </Link>
            </CardContent>
          </Card>

          <Card className="hover:shadow-md transition-shadow">
            <CardHeader>
              <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mb-2">
                <FileText className="h-5 w-5 text-purple-600" />
              </div>
              <CardTitle className="text-base">Order History</CardTitle>
              <CardDescription>Track your orders and quotes</CardDescription>
            </CardHeader>
            <CardContent>
              <Link href="/partner/orders">
                <Button variant="outline" className="w-full">View Orders</Button>
              </Link>
            </CardContent>
          </Card>
        </div>

        {/* Recent Orders */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-base">Recent Orders</CardTitle>
              <Link
                href="/partner/orders"
                className="text-sm text-blue-600 hover:underline flex items-center gap-1"
              >
                View all <ArrowRight className="h-3 w-3" />
              </Link>
            </CardHeader>
            <CardContent>
              {recentOrders?.orders?.length > 0 ? (
                <div className="space-y-2">
                  {recentOrders.orders.slice(0, 5).map((order: any) => (
                    <Link
                      key={order.id}
                      href={`/partner/orders/${order.id}`}
                      className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                    >
                      <div>
                        <div className="font-mono font-medium text-sm">{order.orderNumber}</div>
                        <div className="text-xs text-gray-500">
                          {new Date(order.createdAt).toLocaleDateString()}
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs text-gray-500">{order.itemCount} items</span>
                        <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusBadge(order.status)}`}>
                          {order.status}
                        </span>
                      </div>
                    </Link>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">
                  <Package className="h-10 w-10 mx-auto mb-3 opacity-20" />
                  <p className="text-sm">No orders yet</p>
                  <p className="text-xs mt-1">Start by browsing products</p>
                </div>
              )}
            </CardContent>
          </Card>

          {user.role === 'CompanyAdmin' && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base flex items-center gap-2">
                  <Users className="h-4 w-4" />
                  Company Info
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Company</span>
                  <span className="font-medium">{user.company.name}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Status</span>
                  <Badge className="bg-green-100 text-green-800">{user.company.status}</Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Your Role</span>
                  <span className="text-sm font-medium">{user.role}</span>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

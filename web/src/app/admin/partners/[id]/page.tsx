'use client';

import { use, useState } from 'react';
import { useTranslations } from 'next-intl';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { partnerAdminApi } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { toast } from 'sonner';
import {
  ArrowLeft,
  CheckCircle,
  UserPlus,
  XCircle,
  Building2,
  Mail,
  Phone,
  MapPin,
  Globe,
  Users,
  Calendar,
  FileText,
  Tag,
  CreditCard,
  PlusCircle,
} from 'lucide-react';
import { formatPrice } from '@/lib/utils';

const statusColors = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-red-100 text-red-800',
  Rejected: 'bg-gray-100 text-gray-800',
};

export default function PartnerDetailsPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const t = useTranslations('partners');
  const tc = useTranslations('common');
  const router = useRouter();
  const queryClient = useQueryClient();
  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [suspendDialogOpen, setSuspendDialogOpen] = useState(false);
  const [addUserDialogOpen, setAddUserDialogOpen] = useState(false);
  const [editUserDialogOpen, setEditUserDialogOpen] = useState(false);
  const [resetPasswordDialogOpen, setResetPasswordDialogOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [approvalNotes, setApprovalNotes] = useState('');
  const [suspendReason, setSuspendReason] = useState('');
  const [newUser, setNewUser] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    role: 'User',
    scopes: [] as string[],
  });
  const [editUser, setEditUser] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    role: 'User',
    isActive: true,
    scopes: [] as string[],
  });
  const [newPassword, setNewPassword] = useState('');
  const [editingDiscountRate, setEditingDiscountRate] = useState<number | null>(null);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [showAllTransactions, setShowAllTransactions] = useState(false);
  const [txSortBy, setTxSortBy] = useState<'date' | 'amount'>('date');
  const [txSortDir, setTxSortDir] = useState<'desc' | 'asc'>('desc');
  const [txDateFrom, setTxDateFrom] = useState('');
  const [txDateTo, setTxDateTo] = useState('');
  const [transactionForm, setTransactionForm] = useState({
    type: 'PaymentCredit',
    amount: '',
    paymentMethod: 'Cash',
    orderReference: '',
    notes: '',
  });

  const { data: partner, isLoading } = useQuery({
    queryKey: ['admin-partner', id],
    queryFn: () => partnerAdminApi.getPartnerDetails(id),
  });

  const approveMutation = useMutation({
    mutationFn: () => partnerAdminApi.approvePartner(id, approvalNotes || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
      toast.success('Partner company approved successfully');
      setApproveDialogOpen(false);
      setApprovalNotes('');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Failed to approve partner. Please try again.';
      toast.error(message);
    },
  });

  const suspendMutation = useMutation({
    mutationFn: () => partnerAdminApi.suspendPartner(id, suspendReason || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
      toast.success('Partner company suspended');
      setSuspendDialogOpen(false);
      setSuspendReason('');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Failed to suspend partner. Please try again.';
      toast.error(message);
    },
  });

  const addUserMutation = useMutation({
    mutationFn: () => partnerAdminApi.addPartnerUser(id, newUser),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      toast.success('User added successfully');
      setAddUserDialogOpen(false);
      setNewUser({ firstName: '', lastName: '', email: '', password: '', role: 'User', scopes: [] });
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Failed to add user. Please try again.';
      toast.error(message);
    },
  });

  const updateUserMutation = useMutation({
    mutationFn: () => partnerAdminApi.updatePartnerUser(selectedUserId!, {
      ...editUser,
      phone: editUser.phone || undefined,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      toast.success('User updated successfully');
      setEditUserDialogOpen(false);
      setSelectedUserId(null);
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Failed to update user.';
      toast.error(message);
    },
  });

  const resetPasswordMutation = useMutation({
    mutationFn: () => partnerAdminApi.resetPartnerUserPassword(selectedUserId!, newPassword),
    onSuccess: () => {
      toast.success('Password reset successfully');
      setResetPasswordDialogOpen(false);
      setNewPassword('');
      setSelectedUserId(null);
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Failed to reset password.';
      toast.error(message);
    },
  });

  const updatePricingMutation = useMutation({
    mutationFn: (discountRate: number) => partnerAdminApi.updatePricing(id, discountRate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      toast.success(t('pricingUpdated'));
      setEditingDiscountRate(null);
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || t('pricingUpdateFailed');
      toast.error(message);
    },
  });

  const recordTransactionMutation = useMutation({
    mutationFn: () =>
      partnerAdminApi.recordTransaction(id, {
        type: transactionForm.type,
        amount: parseFloat(transactionForm.amount),
        paymentMethod: transactionForm.type === 'PaymentCredit' ? transactionForm.paymentMethod : undefined,
        orderReference: transactionForm.orderReference || undefined,
        notes: transactionForm.notes || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      toast.success(t('transactionRecorded'));
      setTransactionDialogOpen(false);
      setTransactionForm({ type: 'PaymentCredit', amount: '', paymentMethod: 'Cash', orderReference: '', notes: '' });
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || t('transactionFailed');
      toast.error(message);
    },
  });

  const PARTNER_SCOPES = [
    { value: 'ViewCatalog', label: 'View Catalog' },
    { value: 'ViewPrices', label: 'View Prices' },
    { value: 'ManageCart', label: 'Manage Cart' },
    { value: 'PlaceOrders', label: 'Place Orders' },
    { value: 'ViewOrders', label: 'View Orders' },
    { value: 'ManageTeam', label: 'Manage Team' },
  ];

  function toggleScope(scopes: string[], scope: string): string[] {
    return scopes.includes(scope) ? scopes.filter((s) => s !== scope) : [...scopes, scope];
  }

  function openEditUser(user: { id: string; firstName: string; lastName: string; role: string; isActive: boolean; scopes: string[] }) {
    setSelectedUserId(user.id);
    setEditUser({
      firstName: user.firstName,
      lastName: user.lastName,
      phone: '',
      role: user.role,
      isActive: user.isActive,
      scopes: user.scopes ?? [],
    });
    setEditUserDialogOpen(true);
  }

  function openResetPassword(userId: string) {
    setSelectedUserId(userId);
    setNewPassword('');
    setResetPasswordDialogOpen(true);
  }

  if (isLoading) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900" />
        </div>
      </div>
    );
  }

  if (!partner) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="text-center">
          <h2 className="text-2xl font-bold">Partner Not Found</h2>
          <Button onClick={() => router.push('/admin/partners')} className="mt-4">
            Back to Partners
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      {/* Header */}
      <div className="mb-6">
        <Link href="/admin/partners">
          <Button variant="ghost" size="sm" className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Partners
          </Button>
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-3xl font-bold">{partner.companyName}</h1>
            <p className="text-gray-600 mt-1">Tax ID: {partner.taxId}</p>
          </div>
          <div className="flex gap-2">
            <Badge className={statusColors[partner.status]}>{partner.status}</Badge>
          </div>
        </div>
      </div>

      {/* Actions */}
      {partner.status === 'Pending' && (
        <Card className="mb-6 border-yellow-200 bg-yellow-50">
          <CardHeader>
            <CardTitle>Pending Approval</CardTitle>
            <CardDescription>Review the company details and approve or reject the registration.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex gap-2">
              <Button onClick={() => setApproveDialogOpen(true)}>
                <CheckCircle className="h-4 w-4 mr-2" />
                Approve Partner
              </Button>
              <Button variant="destructive" onClick={() => setSuspendDialogOpen(true)}>
                <XCircle className="h-4 w-4 mr-2" />
                Reject Registration
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {partner.status === 'Active' && (
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Active Partner</CardTitle>
            <CardDescription>This partner company is currently active.</CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="destructive" onClick={() => setSuspendDialogOpen(true)}>
              <XCircle className="h-4 w-4 mr-2" />
              Suspend Partner
            </Button>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Company Information */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Building2 className="h-5 w-5 mr-2" />
                Company Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <div className="text-sm text-gray-600">Company Name</div>
                  <div className="font-medium">{partner.companyName}</div>
                </div>
                <div>
                  <div className="text-sm text-gray-600">Tax ID</div>
                  <div className="font-medium">{partner.taxId}</div>
                </div>
                <div>
                  <div className="text-sm text-gray-600 flex items-center">
                    <Mail className="h-4 w-4 mr-1" />
                    Email
                  </div>
                  <div className="font-medium">{partner.email}</div>
                </div>
                <div>
                  <div className="text-sm text-gray-600 flex items-center">
                    <Phone className="h-4 w-4 mr-1" />
                    Phone
                  </div>
                  <div className="font-medium">{partner.phone}</div>
                </div>
              </div>

              <div>
                <div className="text-sm text-gray-600 flex items-center mb-1">
                  <MapPin className="h-4 w-4 mr-1" />
                  Address
                </div>
                <div className="font-medium">
                  {partner.address}
                  <br />
                  {partner.city}, {partner.state} {partner.postalCode}
                  <br />
                  {partner.country}
                </div>
              </div>

              {partner.website && (
                <div>
                  <div className="text-sm text-gray-600 flex items-center mb-1">
                    <Globe className="h-4 w-4 mr-1" />
                    Website
                  </div>
                  <a
                    href={partner.website}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="font-medium text-blue-600 hover:underline"
                  >
                    {partner.website}
                  </a>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Business Details */}
          <Card>
            <CardHeader>
              <CardTitle>Business Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                {partner.industry && (
                  <div>
                    <div className="text-sm text-gray-600">Industry</div>
                    <div className="font-medium">{partner.industry}</div>
                  </div>
                )}
                {partner.employeeCount && (
                  <div>
                    <div className="text-sm text-gray-600">Employees</div>
                    <div className="font-medium">{partner.employeeCount}</div>
                  </div>
                )}
                {partner.annualRevenue && (
                  <div>
                    <div className="text-sm text-gray-600">Annual Revenue</div>
                    <div className="font-medium">${partner.annualRevenue.toLocaleString()}</div>
                  </div>
                )}
              </div>

              {partner.notes && (
                <div>
                  <div className="text-sm text-gray-600 mb-1">Internal Notes</div>
                  <div className="text-sm bg-gray-50 p-3 rounded">{partner.notes}</div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Users */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center">
                  <Users className="h-5 w-5 mr-2" />
                  Users ({partner.users.length})
                </CardTitle>
                <Button size="sm" onClick={() => setAddUserDialogOpen(true)}>
                  <UserPlus className="h-4 w-4 mr-2" />
                  Add User
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {partner.users.map((user) => (
                  <div key={user.id} className="flex items-center justify-between p-3 bg-gray-50 rounded gap-3">
                    <div className="flex-1 min-w-0">
                      <div className="font-medium flex items-center gap-2">
                        {user.firstName} {user.lastName}
                        {!user.isActive && (
                          <Badge variant="secondary" className="text-xs">Inactive</Badge>
                        )}
                      </div>
                      <div className="text-sm text-gray-600">{user.email}</div>
                      <div className="text-xs text-gray-500">
                        {user.lastLoginAt
                          ? `Last login: ${new Date(user.lastLoginAt).toLocaleDateString()}`
                          : 'Never logged in'}
                      </div>
                      {user.scopes.length > 0 && (
                        <div className="flex flex-wrap gap-1 mt-1">
                          {user.scopes.map((scope) => (
                            <Badge key={scope} variant="outline" className="text-xs px-1 py-0">
                              {scope}
                            </Badge>
                          ))}
                        </div>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant={user.role === 'CompanyAdmin' ? 'default' : 'secondary'}>
                        {user.role}
                      </Badge>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => openEditUser(user)}
                      >
                        {tc('edit')}
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => openResetPassword(user.id)}
                      >
                        Reset Password
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Timeline */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Calendar className="h-5 w-5 mr-2" />
                Timeline
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div>
                <div className="text-sm text-gray-600">Registered</div>
                <div className="font-medium">{new Date(partner.createdAt).toLocaleString()}</div>
              </div>
              {partner.approvedAt && (
                <div>
                  <div className="text-sm text-gray-600">Approved</div>
                  <div className="font-medium">{new Date(partner.approvedAt).toLocaleString()}</div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Approval Info */}
          {partner.approvalNotes && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center">
                  <FileText className="h-5 w-5 mr-2" />
                  Approval Notes
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-sm bg-gray-50 p-3 rounded">{partner.approvalNotes}</div>
              </CardContent>
            </Card>
          )}

          {/* Pricing Policy */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Tag className="h-5 w-5 mr-2" />
                {t('pricingPolicy')}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="text-sm text-gray-600">{t('discountRate')}</div>
              {editingDiscountRate !== null ? (
                <div className="flex items-center gap-2">
                  <Input
                    type="number"
                    min={0}
                    max={100}
                    value={editingDiscountRate}
                    onChange={(e) => setEditingDiscountRate(Number(e.target.value))}
                    className="w-24"
                  />
                  <span className="text-sm">%</span>
                  <Button
                    size="sm"
                    onClick={() => updatePricingMutation.mutate(editingDiscountRate)}
                    disabled={updatePricingMutation.isPending}
                  >
                    {updatePricingMutation.isPending ? t('saving') : tc('save')}
                  </Button>
                  <Button size="sm" variant="outline" onClick={() => setEditingDiscountRate(null)}>
                    {tc('cancel')}
                  </Button>
                </div>
              ) : (
                <div className="flex items-center justify-between">
                  <span className="text-2xl font-bold">{partner.discountRate}%</span>
                  <Button size="sm" variant="outline" onClick={() => setEditingDiscountRate(partner.discountRate)}>
                    {tc('edit')}
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Contacts */}
          {partner.contacts.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Contacts</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {partner.contacts.map((contact) => (
                  <div key={contact.id} className="text-sm">
                    <div className="font-medium flex items-center">
                      {contact.name}
                      {contact.isPrimary && (
                        <Badge variant="secondary" className="ml-2 text-xs">
                          Primary
                        </Badge>
                      )}
                    </div>
                    <div className="text-gray-600">{contact.title}</div>
                    <div className="text-gray-600">{contact.email}</div>
                    <div className="text-gray-600">{contact.phone}</div>
                  </div>
                ))}
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Current Account — full width */}
      <Card className="mt-6">
        <CardHeader>
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-6">
              <CardTitle className="flex items-center">
                <CreditCard className="h-5 w-5 mr-2" />
                Current Account
              </CardTitle>
              <div>
                <div className="text-xs text-gray-500">{t('currentBalance')}</div>
                <div className={`text-xl font-bold ${partner.currentBalance > 0 ? 'text-red-600' : 'text-green-700'}`}>
                  {formatPrice(partner.currentBalance)}
                  {partner.currentBalance > 0 && (
                    <span className="text-xs font-normal text-red-500 ml-1">{t('owes')}</span>
                  )}
                </div>
              </div>
            </div>
            <div className="flex items-center gap-2 flex-wrap">
              {/* Date filters */}
              <div className="flex items-center gap-1">
                <span className="text-xs text-gray-500">From</span>
                <Input
                  type="date"
                  value={txDateFrom}
                  onChange={(e) => setTxDateFrom(e.target.value)}
                  className="h-8 text-xs w-36"
                />
              </div>
              <div className="flex items-center gap-1">
                <span className="text-xs text-gray-500">To</span>
                <Input
                  type="date"
                  value={txDateTo}
                  onChange={(e) => setTxDateTo(e.target.value)}
                  className="h-8 text-xs w-36"
                />
              </div>
              {(txDateFrom || txDateTo) && (
                <Button size="sm" variant="ghost" className="h-8 text-xs" onClick={() => { setTxDateFrom(''); setTxDateTo(''); }}>
                  Clear
                </Button>
              )}
              {/* Sort */}
              {(['date', 'amount'] as const).map((field) => (
                <Button
                  key={field}
                  size="sm"
                  variant={txSortBy === field ? 'default' : 'outline'}
                  className="h-8 text-xs"
                  onClick={() => {
                    if (txSortBy === field) setTxSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
                    else { setTxSortBy(field); setTxSortDir('desc'); }
                  }}
                >
                  {field === 'date' ? 'Date' : 'Amount'}
                  {txSortBy === field ? (txSortDir === 'asc' ? ' ↑' : ' ↓') : ''}
                </Button>
              ))}
              <Button size="sm" onClick={() => setTransactionDialogOpen(true)}>
                <PlusCircle className="h-4 w-4 mr-1" />
                {t('recordTransaction')}
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {(() => {
            let filtered = partner.transactions.filter((tx) => {
              if (txDateFrom && new Date(tx.createdAt) < new Date(txDateFrom)) return false;
              if (txDateTo && new Date(tx.createdAt) > new Date(txDateTo + 'T23:59:59')) return false;
              return true;
            });
            filtered = [...filtered].sort((a, b) => {
              const dir = txSortDir === 'asc' ? 1 : -1;
              if (txSortBy === 'amount') return (a.amount - b.amount) * dir;
              return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * dir;
            });
            const visible = showAllTransactions ? filtered : filtered.slice(0, 10);

            if (filtered.length === 0) {
              return <div className="text-sm text-gray-400 py-6 text-center">No transactions found.</div>;
            }

            return (
              <>
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b text-xs text-gray-500 uppercase">
                      <th className="pb-2 text-left font-medium">Date</th>
                      <th className="pb-2 text-left font-medium">Type</th>
                      <th className="pb-2 text-left font-medium">Order #</th>
                      <th className="pb-2 text-left font-medium">Payment Method</th>
                      <th className="pb-2 text-left font-medium">Notes</th>
                      <th className="pb-2 text-right font-medium">Amount</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {visible.map((tx) => (
                      <tr key={tx.id} className="hover:bg-gray-50">
                        <td className="py-2.5 text-gray-500 whitespace-nowrap">
                          {new Date(tx.createdAt).toLocaleDateString('tr-TR')}
                        </td>
                        <td className="py-2.5">
                          <Badge variant={tx.type === 'PaymentCredit' ? 'default' : 'destructive'} className="text-xs">
                            {tx.type === 'PaymentCredit' ? 'Ödeme' : tx.type === 'OrderDebit' ? 'Sipariş' : 'Düzeltme'}
                          </Badge>
                        </td>
                        <td className="py-2.5 text-blue-600 font-medium">
                          {tx.orderReference ?? '—'}
                        </td>
                        <td className="py-2.5 text-gray-500">
                          {tx.paymentMethod ?? '—'}
                        </td>
                        <td className="py-2.5 text-gray-400 max-w-[220px] truncate">
                          {tx.notes ?? '—'}
                        </td>
                        <td className={`py-2.5 text-right font-semibold whitespace-nowrap ${tx.type === 'PaymentCredit' ? 'text-green-700' : 'text-red-600'}`}>
                          {tx.type === 'PaymentCredit' ? '+' : '−'}{formatPrice(tx.amount)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {filtered.length > 10 && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="w-full mt-3 text-xs"
                    onClick={() => setShowAllTransactions((v) => !v)}
                  >
                    {showAllTransactions ? 'Show less' : `Show all ${filtered.length} transactions`}
                  </Button>
                )}
              </>
            );
          })()}
        </CardContent>
      </Card>

      {/* Approve Dialog */}
      <Dialog open={approveDialogOpen} onOpenChange={setApproveDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Approve Partner Registration</DialogTitle>
            <DialogDescription>
              This will activate the partner company and all its users. They will be able to login and access the system.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Approval Notes (Optional)</label>
              <Textarea
                placeholder="Add notes about the approval..."
                value={approvalNotes}
                onChange={(e) => setApprovalNotes(e.target.value)}
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setApproveDialogOpen(false)}>
              {tc('cancel')}
            </Button>
            <Button onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
              {approveMutation.isPending ? 'Approving...' : 'Approve Partner'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Suspend Dialog */}
      <Dialog open={suspendDialogOpen} onOpenChange={setSuspendDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {partner.status === 'Pending' ? 'Reject Registration' : 'Suspend Partner'}
            </DialogTitle>
            <DialogDescription>
              This will {partner.status === 'Pending' ? 'reject' : 'suspend'} the partner company and deactivate all users.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Reason (Optional)</label>
              <Textarea
                placeholder="Explain why you are suspending this partner..."
                value={suspendReason}
                onChange={(e) => setSuspendReason(e.target.value)}
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setSuspendDialogOpen(false)}>
              {tc('cancel')}
            </Button>
            <Button
              variant="destructive"
              onClick={() => suspendMutation.mutate()}
              disabled={suspendMutation.isPending}
            >
              {suspendMutation.isPending ? 'Processing...' : 'Confirm'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Add User Dialog */}
      <Dialog open={addUserDialogOpen} onOpenChange={setAddUserDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add User to {partner.companyName}</DialogTitle>
            <DialogDescription>
              Create a new user account for this partner company. They can log in immediately.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="new-firstName">First Name *</Label>
                <Input
                  id="new-firstName"
                  value={newUser.firstName}
                  onChange={(e) => setNewUser((u) => ({ ...u, firstName: e.target.value }))}
                  placeholder="John"
                />
              </div>
              <div>
                <Label htmlFor="new-lastName">Last Name *</Label>
                <Input
                  id="new-lastName"
                  value={newUser.lastName}
                  onChange={(e) => setNewUser((u) => ({ ...u, lastName: e.target.value }))}
                  placeholder="Smith"
                />
              </div>
            </div>
            <div>
              <Label htmlFor="new-email">Email Address *</Label>
              <Input
                id="new-email"
                type="email"
                value={newUser.email}
                onChange={(e) => setNewUser((u) => ({ ...u, email: e.target.value }))}
                placeholder="john@company.com"
              />
            </div>
            <div>
              <Label htmlFor="new-password">Password *</Label>
              <Input
                id="new-password"
                type="password"
                value={newUser.password}
                onChange={(e) => setNewUser((u) => ({ ...u, password: e.target.value }))}
                placeholder="Min 8 chars, uppercase, number, special char"
              />
            </div>
            <div>
              <Label htmlFor="new-role">Role *</Label>
              <Select
                value={newUser.role}
                onValueChange={(value) => setNewUser((u) => ({ ...u, role: value }))}
              >
                <SelectTrigger id="new-role">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="CompanyAdmin">Company Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label>Permissions</Label>
              <div className="grid grid-cols-2 gap-2 mt-2">
                {PARTNER_SCOPES.map((scope) => (
                  <label key={scope.value} className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      className="h-4 w-4"
                      checked={newUser.scopes.includes(scope.value)}
                      onChange={() => setNewUser((u) => ({ ...u, scopes: toggleScope(u.scopes, scope.value) }))}
                    />
                    <span className="text-sm">{scope.label}</span>
                  </label>
                ))}
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddUserDialogOpen(false)}>
              {tc('cancel')}
            </Button>
            <Button
              onClick={() => addUserMutation.mutate()}
              disabled={
                addUserMutation.isPending ||
                !newUser.firstName ||
                !newUser.lastName ||
                !newUser.email ||
                !newUser.password
              }
            >
              {addUserMutation.isPending ? 'Adding...' : 'Add User'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog open={editUserDialogOpen} onOpenChange={setEditUserDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit User</DialogTitle>
            <DialogDescription>Update the user&apos;s details and permissions.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="edit-firstName">First Name *</Label>
                <Input
                  id="edit-firstName"
                  value={editUser.firstName}
                  onChange={(e) => setEditUser((u) => ({ ...u, firstName: e.target.value }))}
                />
              </div>
              <div>
                <Label htmlFor="edit-lastName">Last Name *</Label>
                <Input
                  id="edit-lastName"
                  value={editUser.lastName}
                  onChange={(e) => setEditUser((u) => ({ ...u, lastName: e.target.value }))}
                />
              </div>
            </div>
            <div>
              <Label htmlFor="edit-phone">Phone</Label>
              <Input
                id="edit-phone"
                value={editUser.phone}
                onChange={(e) => setEditUser((u) => ({ ...u, phone: e.target.value }))}
                placeholder="+1-555-0100"
              />
            </div>
            <div>
              <Label htmlFor="edit-role">Role *</Label>
              <Select
                value={editUser.role}
                onValueChange={(value) => setEditUser((u) => ({ ...u, role: value }))}
              >
                <SelectTrigger id="edit-role">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="CompanyAdmin">Company Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label>Permissions</Label>
              <div className="grid grid-cols-2 gap-2 mt-2">
                {PARTNER_SCOPES.map((scope) => (
                  <label key={scope.value} className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      className="h-4 w-4"
                      checked={editUser.scopes.includes(scope.value)}
                      onChange={() => setEditUser((u) => ({ ...u, scopes: toggleScope(u.scopes, scope.value) }))}
                    />
                    <span className="text-sm">{scope.label}</span>
                  </label>
                ))}
              </div>
            </div>
            <div className="flex items-center gap-3">
              <input
                id="edit-isActive"
                type="checkbox"
                checked={editUser.isActive}
                onChange={(e) => setEditUser((u) => ({ ...u, isActive: e.target.checked }))}
                className="h-4 w-4"
              />
              <Label htmlFor="edit-isActive">Account Active</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditUserDialogOpen(false)}>{tc('cancel')}</Button>
            <Button
              onClick={() => updateUserMutation.mutate()}
              disabled={updateUserMutation.isPending || !editUser.firstName || !editUser.lastName}
            >
              {updateUserMutation.isPending ? t('saving') : tc('save')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Record Transaction Dialog */}
      <Dialog open={transactionDialogOpen} onOpenChange={setTransactionDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Record Account Transaction</DialogTitle>
            <DialogDescription>
              Record a payment received or a debit to the partner&apos;s account.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>{t('transactionType')}</Label>
              <Select
                value={transactionForm.type}
                onValueChange={(v) => setTransactionForm((f) => ({ ...f, type: v }))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="PaymentCredit">{t('paymentCredit')}</SelectItem>
                  <SelectItem value="OrderDebit">{t('orderDebit')}</SelectItem>
                  <SelectItem value="ManualAdjustment">{t('manualAdjustment')}</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label>{t('amount')}</Label>
              <Input
                type="number"
                min={0}
                step={0.01}
                value={transactionForm.amount}
                onChange={(e) => setTransactionForm((f) => ({ ...f, amount: e.target.value }))}
                placeholder="0.00"
              />
            </div>
            {transactionForm.type === 'PaymentCredit' && (
              <div>
                <Label>{t('paymentMethod')}</Label>
                <Select
                  value={transactionForm.paymentMethod}
                  onValueChange={(v) => setTransactionForm((f) => ({ ...f, paymentMethod: v }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Cash">{t('cash')}</SelectItem>
                    <SelectItem value="Check">{t('check')}</SelectItem>
                    <SelectItem value="PromissoryNote">{t('promissoryNote')}</SelectItem>
                    <SelectItem value="BankTransfer">{t('bankTransfer')}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            )}
            <div>
              <Label>{t('orderReference')}</Label>
              <Input
                value={transactionForm.orderReference}
                onChange={(e) => setTransactionForm((f) => ({ ...f, orderReference: e.target.value }))}
                placeholder="ORD-2026-0001"
              />
            </div>
            <div>
              <Label>{t('notes')}</Label>
              <Textarea
                value={transactionForm.notes}
                onChange={(e) => setTransactionForm((f) => ({ ...f, notes: e.target.value }))}
                rows={2}
                placeholder="Additional notes..."
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setTransactionDialogOpen(false)}>{tc('cancel')}</Button>
            <Button
              onClick={() => recordTransactionMutation.mutate()}
              disabled={recordTransactionMutation.isPending || !transactionForm.amount || parseFloat(transactionForm.amount) <= 0}
            >
              {recordTransactionMutation.isPending ? t('saving') : t('saveTransaction')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reset Password Dialog */}
      <Dialog open={resetPasswordDialogOpen} onOpenChange={setResetPasswordDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reset Password</DialogTitle>
            <DialogDescription>
              Set a new password for this user. They can log in immediately with the new password.
            </DialogDescription>
          </DialogHeader>
          <div>
            <Label htmlFor="reset-password">New Password *</Label>
            <Input
              id="reset-password"
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Min 8 chars, uppercase, number, special char"
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setResetPasswordDialogOpen(false)}>{tc('cancel')}</Button>
            <Button
              onClick={() => resetPasswordMutation.mutate()}
              disabled={resetPasswordMutation.isPending || newPassword.length < 8}
            >
              {resetPasswordMutation.isPending ? 'Resetting...' : 'Reset Password'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

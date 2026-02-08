'use client';

import { useState } from 'react';
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
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/hooks/use-toast';
import {
  ArrowLeft,
  CheckCircle,
  XCircle,
  Building2,
  Mail,
  Phone,
  MapPin,
  Globe,
  Users,
  Calendar,
  FileText,
} from 'lucide-react';

const statusColors = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-red-100 text-red-800',
  Rejected: 'bg-gray-100 text-gray-800',
};

export default function PartnerDetailsPage({ params }: { params: { id: string } }) {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [suspendDialogOpen, setSuspendDialogOpen] = useState(false);
  const [approvalNotes, setApprovalNotes] = useState('');
  const [suspendReason, setSuspendReason] = useState('');

  const { data: partner, isLoading } = useQuery({
    queryKey: ['admin-partner', params.id],
    queryFn: () => partnerAdminApi.getPartnerDetails(params.id),
  });

  const approveMutation = useMutation({
    mutationFn: () => partnerAdminApi.approvePartner(params.id, approvalNotes || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', params.id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
      toast({
        title: 'Partner Approved',
        description: 'The partner company has been approved successfully.',
      });
      setApproveDialogOpen(false);
      setApprovalNotes('');
    },
    onError: () => {
      toast({
        title: 'Error',
        description: 'Failed to approve partner. Please try again.',
        variant: 'destructive',
      });
    },
  });

  const suspendMutation = useMutation({
    mutationFn: () => partnerAdminApi.suspendPartner(params.id, suspendReason || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', params.id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
      toast({
        title: 'Partner Suspended',
        description: 'The partner company has been suspended.',
      });
      setSuspendDialogOpen(false);
      setSuspendReason('');
    },
    onError: () => {
      toast({
        title: 'Error',
        description: 'Failed to suspend partner. Please try again.',
        variant: 'destructive',
      });
    },
  });

  if (isLoading) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
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
              <CardTitle className="flex items-center">
                <Users className="h-5 w-5 mr-2" />
                Users ({partner.users.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {partner.users.map((user) => (
                  <div key={user.id} className="flex items-center justify-between p-3 bg-gray-50 rounded">
                    <div>
                      <div className="font-medium">
                        {user.firstName} {user.lastName}
                      </div>
                      <div className="text-sm text-gray-600">{user.email}</div>
                    </div>
                    <div className="text-right">
                      <Badge variant={user.role === 'CompanyAdmin' ? 'default' : 'secondary'}>
                        {user.role}
                      </Badge>
                      <div className="text-xs text-gray-500 mt-1">
                        {user.lastLoginAt
                          ? `Last login: ${new Date(user.lastLoginAt).toLocaleDateString()}`
                          : 'Never logged in'}
                      </div>
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
              Cancel
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
              Cancel
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
    </div>
  );
}

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { partnerAdminApi } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Search, Eye, Clock, CheckCircle, XCircle, AlertCircle, Building2 } from 'lucide-react';

const statusColors = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-red-100 text-red-800',
  Rejected: 'bg-gray-100 text-gray-800',
};

const statusIcons = {
  Pending: Clock,
  Active: CheckCircle,
  Suspended: XCircle,
  Rejected: AlertCircle,
};

export default function AdminPartnersPage() {
  const t = useTranslations('partners');
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin-partners', searchTerm, statusFilter, page],
    queryFn: () => partnerAdminApi.getPartners(searchTerm, statusFilter, page, pageSize),
  });

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setPage(1);
  };

  const handleStatusFilter = (value: string) => {
    setStatusFilter(value === 'all' ? '' : value);
    setPage(1);
  };

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold">{t('title')}</h1>
          <p className="text-gray-600 mt-2">{t('titleDesc')}</p>
        </div>
        <Link href="/admin/partners/new">
          <Button>
            <Building2 className="h-4 w-4 mr-2" />
            {t('createPartner')}
          </Button>
        </Link>
      </div>

      {/* Filters */}
      <div className="bg-white p-6 rounded-lg shadow-sm mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Search */}
          <div className="md:col-span-2">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder={t('searchPlaceholder')}
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>

          {/* Status Filter */}
          <Select value={statusFilter || 'all'} onValueChange={handleStatusFilter}>
            <SelectTrigger>
              <SelectValue placeholder={t('filterByStatus')} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              <SelectItem value="Pending">Pending</SelectItem>
              <SelectItem value="Active">Active</SelectItem>
              <SelectItem value="Suspended">Suspended</SelectItem>
              <SelectItem value="Rejected">Rejected</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Stats */}
      {data && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div className="bg-white p-4 rounded-lg shadow-sm">
            <div className="text-sm text-gray-600">{t('totalPartners')}</div>
            <div className="text-2xl font-bold">{data.totalCount}</div>
          </div>
          <div className="bg-yellow-50 p-4 rounded-lg shadow-sm">
            <div className="text-sm text-yellow-800">{t('pendingApproval')}</div>
            <div className="text-2xl font-bold text-yellow-900">
              {data.items.filter((p) => p.status === 'Pending').length}
            </div>
          </div>
          <div className="bg-green-50 p-4 rounded-lg shadow-sm">
            <div className="text-sm text-green-800">{t('active')}</div>
            <div className="text-2xl font-bold text-green-900">
              {data.items.filter((p) => p.status === 'Active').length}
            </div>
          </div>
          <div className="bg-red-50 p-4 rounded-lg shadow-sm">
            <div className="text-sm text-red-800">{t('suspended')}</div>
            <div className="text-2xl font-bold text-red-900">
              {data.items.filter((p) => p.status === 'Suspended').length}
            </div>
          </div>
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg shadow-sm">
        {isLoading ? (
          <div className="p-8 text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto" />
            <p className="mt-4 text-gray-600">{t('loading')}</p>
          </div>
        ) : error ? (
          <div className="p-8 text-center text-red-600">
            {t('loadError')}
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="p-8 text-center text-gray-600">
            {t('noFound')}
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('company')}</TableHead>
                  <TableHead>{t('location')}</TableHead>
                  <TableHead>{t('contact')}</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>{t('users')}</TableHead>
                  <TableHead>{t('registered')}</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((partner) => {
                  const StatusIcon = statusIcons[partner.status];
                  return (
                    <TableRow key={partner.id}>
                      <TableCell>
                        <div>
                          <div className="font-medium">{partner.companyName}</div>
                          <div className="text-sm text-gray-500">{t('taxId')} {partner.taxId}</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm">
                          {partner.city}, {partner.state}
                          <br />
                          {partner.country}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm">
                          <div>{partner.email}</div>
                          <div className="text-gray-500">{partner.phone}</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className={statusColors[partner.status]}>
                          <StatusIcon className="h-3 w-3 mr-1" />
                          {partner.status}
                        </Badge>
                      </TableCell>
                      <TableCell>{partner.userCount}</TableCell>
                      <TableCell>
                        <div className="text-sm">
                          {new Date(partner.createdAt).toLocaleDateString()}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <Link href={`/admin/partners/${partner.id}`}>
                          <Button variant="ghost" size="sm">
                            <Eye className="h-4 w-4 mr-1" />
                            {t('view')}
                          </Button>
                        </Link>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="flex items-center justify-between px-6 py-4 border-t">
                <div className="text-sm text-gray-600">
                  Showing {(page - 1) * pageSize + 1} to{' '}
                  {Math.min(page * pageSize, data.totalCount)} of {data.totalCount} partners
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(page - 1)}
                    disabled={page === 1}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(page + 1)}
                    disabled={page === data.totalPages}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

import { api } from '../api';
import type { OrderDetail, OrderStats, PagedResult } from '../types';

export interface AdminOrderSummary {
  id: string;
  orderNumber: string;
  status: string;
  itemCount: number;
  totalAmount: number | null;
  currency: string;
  partnerCompanyName: string;
  createdAt: string;
}

export interface AdminPartner {
  id: string;
  companyName: string;
  taxId: string;
  email: string;
  phone: string;
  city: string;
  country: string;
  status: string;
  userCount: number;
  createdAt: string;
}

export interface AdminPartnerDetails extends AdminPartner {
  address: string;
  state: string;
  postalCode: string;
  notes: string | null;
  approvedAt: string | null;
  approvedBy: string | null;
  approvalNotes: string | null;
  discountRate: number;
  currentBalance: number;
  transactions: PartnerAccountTransactionDto[];
  users: AdminPartnerUser[];
}

export interface AdminPartnerUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  lastLoginAt: string | null;
}

export interface PartnerAccountTransactionDto {
  id: string;
  type: string;
  amount: number;
  paymentMethod: string | null;
  orderReference: string | null;
  notes: string | null;
  createdBy: string;
  createdAt: string;
}

export const adminApi = {
  // Orders
  getOrders: (params: { status?: string; pageNumber?: number; pageSize?: number } = {}) =>
    api.get<PagedResult<AdminOrderSummary>>('/api/admin/orders', {
      params: {
        status: params.status || undefined,
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 20,
      },
    }),

  getOrderStats: () => api.get<OrderStats>('/api/admin/orders/stats'),

  getOrderDetails: (id: string) =>
    api.get<OrderDetail>(`/api/admin/orders/${id}`),

  updateOrderStatus: (id: string, newStatus: string, notes?: string) =>
    api.put(`/api/admin/orders/${id}/status`, { newStatus, notes }),

  addComment: (id: string, content: string, isInternal: boolean) =>
    api.post(`/api/admin/orders/${id}/comments`, { content, type: 'General', isInternal }),

  // Partners
  getPartners: (params: { searchTerm?: string; status?: string; pageNumber?: number; pageSize?: number } = {}) =>
    api.get<PagedResult<AdminPartner>>('/api/identity/admin/partners', { params }),

  getPartnerDetails: (id: string) =>
    api.get<AdminPartnerDetails>(`/api/identity/admin/partners/${id}`),

  approvePartner: (id: string, notes?: string) =>
    api.put(`/api/identity/admin/partners/${id}/approve`, { approvalNotes: notes }),

  suspendPartner: (id: string, reason?: string) =>
    api.put(`/api/identity/admin/partners/${id}/suspend`, { reason }),

  updatePartnerPricing: (id: string, discountRate: number) =>
    api.put(`/api/identity/admin/partners/${id}/pricing`, { discountRate }),

  recordPartnerTransaction: (id: string, data: {
    type: string;
    amount: number;
    paymentMethod?: string;
    orderReference?: string;
    notes?: string;
  }) => api.post(`/api/identity/admin/partners/${id}/account/transactions`, data),
};

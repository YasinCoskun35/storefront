import { api } from '../api';
import type {
  AddToCartRequest,
  Cart,
  CreateOrderRequest,
  CreateSavedAddressRequest,
  OrderDetail,
  OrderStats,
  OrderSummary,
  PagedResult,
  SavedAddress,
} from '../types';

export interface GetOrdersParams {
  pageNumber?: number;
  pageSize?: number;
  status?: string;
}

export const ordersApi = {
  // Cart
  getCart: () => api.get<Cart>('/api/partner/cart'),

  addToCart: (item: AddToCartRequest) =>
    api.post<{ id: string }>('/api/partner/cart/items', item),

  updateCartItemQuantity: (itemId: string, quantity: number) =>
    api.patch(`/api/partner/cart/items/${itemId}`, { quantity }),

  removeCartItem: (itemId: string) =>
    api.delete(`/api/partner/cart/items/${itemId}`),

  // Orders
  getOrders: (params: GetOrdersParams = {}) =>
    api.get<PagedResult<OrderSummary>>('/api/partner/orders', {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 20,
        status: params.status || undefined,
      },
    }),

  getOrderStats: () => api.get<OrderStats>('/api/partner/orders/stats'),

  getOrderDetails: (id: string) =>
    api.get<OrderDetail>(`/api/partner/orders/${id}`),

  createOrder: (data: CreateOrderRequest) =>
    api.post<{ orderId: string }>('/api/partner/orders', data),

  addComment: (orderId: string, content: string) =>
    api.post(`/api/partner/orders/${orderId}/comments`, {
      content,
      type: 'General',
    }),

  cancelOrder: (orderId: string, reason: string) =>
    api.post(`/api/partner/orders/${orderId}/cancel`, { reason }),

  // Saved addresses
  getSavedAddresses: () =>
    api.get<SavedAddress[]>('/api/partner/saved-addresses'),

  createSavedAddress: (data: CreateSavedAddressRequest) =>
    api.post<{ id: string }>('/api/partner/saved-addresses', data),

  deleteSavedAddress: (id: string) =>
    api.delete(`/api/partner/saved-addresses/${id}`),
};

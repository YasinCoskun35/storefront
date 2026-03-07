import { api } from '../api';

// ============================================
// Types
// ============================================

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl?: string;
  quantity: number;
  selectedVariants?: string | null;
  customizationNotes?: string;
}

export interface Cart {
  id: string;
  itemCount: number;
  items: CartItem[];
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl?: string;
  quantity: number;
  selectedVariants?: string | null;
  unitPrice?: number;
  discount?: number;
  totalPrice?: number;
  customizationNotes?: string;
}

export interface OrderComment {
  id: string;
  content: string;
  type: string;
  authorName: string;
  authorType: string;
  isInternal: boolean;
  attachmentUrl?: string;
  attachmentFileName?: string;
  createdAt: string;
}

export interface OrderSummary {
  id: string;
  orderNumber: string;
  status: string;
  itemCount: number;
  totalAmount?: number;
  currency?: string;
  createdAt: string;
  requestedDeliveryDate?: string;
  hasUnreadComments: boolean;
}

export interface OrderDetails {
  id: string;
  orderNumber: string;
  status: string;
  partnerCompanyId: string;
  partnerCompanyName: string;
  subTotal?: number;
  taxAmount?: number;
  shippingCost?: number;
  discount?: number;
  totalAmount?: number;
  currency?: string;
  deliveryAddress: string;
  deliveryCity: string;
  deliveryState: string;
  deliveryPostalCode: string;
  deliveryCountry: string;
  deliveryNotes?: string;
  requestedDeliveryDate?: string;
  expectedDeliveryDate?: string;
  trackingNumber?: string;
  shippingProvider?: string;
  notes?: string;
  createdAt: string;
  submittedAt?: string;
  confirmedAt?: string;
  items: OrderItem[];
  comments: OrderComment[];
}

export interface CreateOrderRequest {
  deliveryAddress: string;
  deliveryCity: string;
  deliveryState: string;
  deliveryPostalCode: string;
  deliveryCountry: string;
  deliveryNotes?: string;
  requestedDeliveryDate?: string;
  notes?: string;
}

export interface AddToCartRequest {
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl?: string;
  quantity: number;
  selectedVariants?: string;
  customizationNotes?: string;
}

export interface AddCommentRequest {
  content: string;
  type: number;
  attachmentUrl?: string;
  attachmentFileName?: string;
}

// ============================================
// Partner API
// ============================================

export const partnerOrdersApi = {
  // Cart
  async getCart() {
    const response = await api.get<Cart>('/api/partner/cart');
    return response.data;
  },

  async addToCart(request: AddToCartRequest) {
    const response = await api.post('/api/partner/cart/items', request);
    return response.data;
  },

  async removeCartItem(itemId: string) {
    await api.delete(`/api/partner/cart/items/${itemId}`);
  },

  async updateCartItemQuantity(itemId: string, quantity: number) {
    const response = await api.patch(`/api/partner/cart/items/${itemId}`, { quantity });
    return response.data;
  },

  // Orders
  async getOrders(params?: {
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }) {
    const response = await api.get('/api/partner/orders', { params });
    return response.data;
  },

  async getOrderDetails(orderId: string) {
    const response = await api.get<OrderDetails>(`/api/partner/orders/${orderId}`);
    return response.data;
  },

  async createOrder(request: CreateOrderRequest) {
    const response = await api.post('/api/partner/orders', request);
    return response.data;
  },

  async addComment(orderId: string, request: AddCommentRequest) {
    const response = await api.post(`/api/partner/orders/${orderId}/comments`, request);
    return response.data;
  },

  async getStats(): Promise<{ totalOrders: number; pendingOrders: number; activeOrders: number; completedOrders: number }> {
    const response = await api.get('/api/partner/orders/stats');
    return response.data;
  },

  async cancelOrder(orderId: string, reason: string) {
    const response = await api.post(`/api/partner/orders/${orderId}/cancel`, { reason });
    return response.data;
  },
};

// ============================================
// Admin API
// ============================================

export const adminOrdersApi = {
  // Stats
  async getStats(): Promise<{ totalOrders: number; pendingOrders: number; activeOrders: number; completedOrders: number; totalPartners: number }> {
    const response = await api.get('/api/admin/orders/stats');
    return response.data;
  },

  // Orders
  async getOrders(params?: {
    status?: string;
    partnerCompanyId?: string;
    pageNumber?: number;
    pageSize?: number;
  }) {
    const response = await api.get('/api/admin/orders', { params });
    return response.data;
  },

  async getOrderDetails(orderId: string) {
    const response = await api.get<OrderDetails>(`/api/admin/orders/${orderId}`);
    return response.data;
  },

  async updateOrderStatus(orderId: string, newStatus: number, notes?: string) {
    const response = await api.put(`/api/admin/orders/${orderId}/status`, { newStatus, notes });
    return response.data;
  },

  async setOrderPricing(orderId: string, pricing: {
    items: { orderItemId: string; unitPrice: number; discount?: number }[];
    shippingCost?: number;
    taxAmount?: number;
    discount?: number;
    currency?: string;
    notes?: string;
  }) {
    const response = await api.put(`/api/admin/orders/${orderId}/pricing`, pricing);
    return response.data;
  },

  async addComment(
    orderId: string,
    content: string,
    type: number,
    isInternal: boolean,
    attachmentUrl?: string,
    attachmentFileName?: string
  ) {
    const response = await api.post(`/api/admin/orders/${orderId}/comments`, {
      content, type, isInternal, attachmentUrl, attachmentFileName,
    });
    return response.data;
  },

  async updateShipping(orderId: string, data: {
    trackingNumber: string;
    shippingProvider: string;
    expectedDeliveryDate?: string;
    shippingNotes?: string;
  }) {
    const response = await api.put(`/api/admin/orders/${orderId}/shipping`, data);
    return response.data;
  },
};

// ============================================
// Order Status Enum
// ============================================

export enum OrderStatus {
  Draft = 0,
  Pending = 1,
  QuoteSent = 2,
  Confirmed = 3,
  Preparing = 4,
  QualityCheck = 5,
  ReadyToShip = 6,
  Shipping = 7,
  Delivered = 8,
  Cancelled = 9,
  Rejected = 10,
}

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'Draft',
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.QuoteSent]: 'Quote Sent',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Preparing]: 'Preparing',
  [OrderStatus.QualityCheck]: 'Quality Check',
  [OrderStatus.ReadyToShip]: 'Ready to Ship',
  [OrderStatus.Shipping]: 'Shipping',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.Rejected]: 'Rejected',
};

export const ORDER_STATUS_COLORS: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'bg-gray-100 text-gray-800',
  [OrderStatus.Pending]: 'bg-yellow-100 text-yellow-800',
  [OrderStatus.QuoteSent]: 'bg-blue-100 text-blue-800',
  [OrderStatus.Confirmed]: 'bg-green-100 text-green-800',
  [OrderStatus.Preparing]: 'bg-purple-100 text-purple-800',
  [OrderStatus.QualityCheck]: 'bg-indigo-100 text-indigo-800',
  [OrderStatus.ReadyToShip]: 'bg-cyan-100 text-cyan-800',
  [OrderStatus.Shipping]: 'bg-blue-100 text-blue-800',
  [OrderStatus.Delivered]: 'bg-green-100 text-green-800',
  [OrderStatus.Cancelled]: 'bg-red-100 text-red-800',
  [OrderStatus.Rejected]: 'bg-red-100 text-red-800',
};

// ============================================
// Comment Type Enum
// ============================================

export enum CommentType {
  General = 0,
  StatusChange = 1,
  Quote = 2,
  Payment = 3,
  Shipping = 4,
  Internal = 5,
}

export const COMMENT_TYPE_LABELS: Record<CommentType, string> = {
  [CommentType.General]: 'General',
  [CommentType.StatusChange]: 'Status Change',
  [CommentType.Quote]: 'Quote',
  [CommentType.Payment]: 'Payment',
  [CommentType.Shipping]: 'Shipping',
  [CommentType.Internal]: 'Internal',
};

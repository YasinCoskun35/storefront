import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080';

// ============================================
// Types
// ============================================

export interface ColorChart {
  id: string;
  name: string;
  code: string;
  description: string;
  type: string;
  mainImageUrl?: string;
  thumbnailUrl?: string;
  isActive: boolean;
  createdAt: string;
}

export interface ColorOption {
  id: string;
  colorChartId: string;
  name: string;
  code: string;
  hexColor?: string;
  imageUrl?: string;
  isAvailable: boolean;
  stockLevel: number;
  priceAdjustment?: number;
  displayOrder: number;
}

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl?: string;
  quantity: number;
  colorChartId?: string;
  colorChartName?: string;
  colorOptionId?: string;
  colorOptionName?: string;
  colorOptionCode?: string;
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
  colorChartName?: string;
  colorOptionName?: string;
  colorOptionCode?: string;
  colorOptionImageUrl?: string;
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
  colorChartId?: string;
  colorChartName?: string;
  colorOptionId?: string;
  colorOptionName?: string;
  colorOptionCode?: string;
  customizationNotes?: string;
}

export interface AddCommentRequest {
  content: string;
  type: number; // CommentType enum
  attachmentUrl?: string;
  attachmentFileName?: string;
}

export interface CreateColorChartRequest {
  name: string;
  code: string;
  description: string;
  type: string;
  mainImageUrl?: string;
  thumbnailUrl?: string;
}

export interface AddColorOptionRequest {
  name: string;
  code: string;
  hexColor?: string;
  imageUrl?: string;
  priceAdjustment?: number;
  displayOrder: number;
}

// ============================================
// Partner API
// ============================================

export const partnerOrdersApi = {
  // Cart
  async getCart() {
    const response = await axios.get<Cart>(`${API_URL}/api/partner/cart`, {
      withCredentials: true,
    });
    return response.data;
  },

  async addToCart(request: AddToCartRequest) {
    const response = await axios.post(
      `${API_URL}/api/partner/cart/items`,
      request,
      { withCredentials: true }
    );
    return response.data;
  },

  // Orders
  async getOrders(params?: {
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }) {
    const response = await axios.get(`${API_URL}/api/partner/orders`, {
      params,
      withCredentials: true,
    });
    return response.data;
  },

  async getOrderDetails(orderId: string) {
    const response = await axios.get<OrderDetails>(
      `${API_URL}/api/partner/orders/${orderId}`,
      { withCredentials: true }
    );
    return response.data;
  },

  async createOrder(request: CreateOrderRequest) {
    const response = await axios.post(
      `${API_URL}/api/partner/orders`,
      request,
      { withCredentials: true }
    );
    return response.data;
  },

  async addComment(orderId: string, request: AddCommentRequest) {
    const response = await axios.post(
      `${API_URL}/api/partner/orders/${orderId}/comments`,
      request,
      { withCredentials: true }
    );
    return response.data;
  },
};

// ============================================
// Admin API
// ============================================

export const adminOrdersApi = {
  // Orders
  async getOrders(params?: {
    status?: string;
    partnerCompanyId?: string;
    pageNumber?: number;
    pageSize?: number;
  }) {
    const response = await axios.get(`${API_URL}/api/admin/orders`, {
      params,
      withCredentials: true,
    });
    return response.data;
  },

  async getOrderDetails(orderId: string) {
    const response = await axios.get<OrderDetails>(
      `${API_URL}/api/admin/orders/${orderId}`,
      { withCredentials: true }
    );
    return response.data;
  },

  async updateOrderStatus(
    orderId: string,
    newStatus: number,
    notes?: string
  ) {
    const response = await axios.put(
      `${API_URL}/api/admin/orders/${orderId}/status`,
      { newStatus, notes },
      { withCredentials: true }
    );
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
    const response = await axios.post(
      `${API_URL}/api/admin/orders/${orderId}/comments`,
      { content, type, isInternal, attachmentUrl, attachmentFileName },
      { withCredentials: true }
    );
    return response.data;
  },

  // Color Charts
  async createColorChart(request: CreateColorChartRequest) {
    const response = await axios.post(
      `${API_URL}/api/admin/color-charts`,
      request,
      { withCredentials: true }
    );
    return response.data;
  },

  async addColorOption(chartId: string, request: AddColorOptionRequest) {
    const response = await axios.post(
      `${API_URL}/api/admin/color-charts/${chartId}/options`,
      request,
      { withCredentials: true }
    );
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

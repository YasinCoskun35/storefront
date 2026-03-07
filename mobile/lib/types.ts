// Auth
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface AdminLoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    roles: string[];
  };
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isAdmin?: boolean;
  company: Company;
}

export interface Company {
  id: string;
  name: string;
  status: string;
}

// Pagination
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Catalog
export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  productCount: number;
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  description: string | null;
  specifications: string | null;
  categoryId: string;
  categoryName: string;
  primaryImageUrl: string | null;
  imageUrls: string[];
  isActive: boolean;
  createdAt: string;
}

// Variants
export interface VariantOption {
  id: string;
  variantGroupId: string;
  name: string;
  code: string;
  hexColor?: string;
  imageUrl?: string;
  priceAdjustment?: number;
  isAvailable: boolean;
  displayOrder: number;
}

export interface VariantGroup {
  id: string;
  name: string;
  description: string;
  displayType: 'Swatch' | 'Dropdown' | 'RadioButtons' | 'ImageGrid';
  isRequired: boolean;
  allowMultiple: boolean;
  displayOrder: number;
  isActive: boolean;
  options: VariantOption[];
}

export interface ProductVariantGroup {
  id: string;
  productId: string;
  variantGroupId: string;
  isRequired: boolean;
  displayOrder: number;
  variantGroup: VariantGroup;
}

export interface SelectedVariantItem {
  groupId: string;
  groupName: string;
  optionId: string;
  optionName: string;
  optionCode: string;
  hexColor?: string;
}

// Cart
export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl: string | null;
  quantity: number;
  selectedVariants: string | null;
  customizationNotes: string | null;
}

export interface Cart {
  id: string;
  partnerId: string;
  items: CartItem[];
  itemCount: number;
}

export interface AddToCartRequest {
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl: string | null;
  quantity: number;
  selectedVariants?: string;
  customizationNotes?: string;
}

// Orders
export type OrderStatus =
  | 'Pending'
  | 'QuoteSent'
  | 'Confirmed'
  | 'InProduction'
  | 'ReadyToShip'
  | 'Shipping'
  | 'Delivered'
  | 'Completed'
  | 'Cancelled';

export interface OrderSummary {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  itemCount: number;
  createdAt: string;
  updatedAt: string;
  partnerCompanyName: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl: string | null;
  quantity: number;
  selectedVariants: string | null;
  customizationNotes: string | null;
}

export interface OrderComment {
  id: string;
  content: string;
  type: string;
  authorName: string;
  createdAt: string;
}

export interface ShippingInfo {
  carrier: string | null;
  trackingNumber: string | null;
  shippedAt: string | null;
  estimatedDelivery: string | null;
}

export interface OrderDetail {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  partnerCompanyName: string;
  deliveryAddress: string;
  deliveryCity: string;
  deliveryState: string;
  deliveryPostalCode: string;
  deliveryCountry: string;
  deliveryNotes: string | null;
  requestedDeliveryDate: string | null;
  notes: string | null;
  items: OrderItem[];
  comments: OrderComment[];
  shippingInfo: ShippingInfo | null;
  createdAt: string;
  updatedAt: string;
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

export interface OrderStats {
  totalOrders: number;
  pendingOrders: number;
  activeOrders: number;
  completedOrders: number;
}

// Saved addresses
export interface SavedAddress {
  id: string;
  label: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

export interface CreateSavedAddressRequest {
  label: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

// Partner profile
export interface PartnerProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  role: string;
  company: {
    id: string;
    name: string;
    status: string;
    address: string | null;
    phone: string | null;
    email: string | null;
  };
}

import { api } from '../api';

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
  optionCount?: number;
  createdAt?: string;
  updatedAt?: string;
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

// Selected variant item stored as JSON in cart/order
export interface SelectedVariantItem {
  groupId: string;
  groupName: string;
  optionId: string;
  optionName: string;
  optionCode: string;
  hexColor?: string;
}

export const publicVariantsApi = {
  async getProductVariantGroups(productId: string): Promise<ProductVariantGroup[]> {
    const response = await api.get<ProductVariantGroup[]>(`/api/products/${productId}/variant-groups`);
    return response.data;
  },
};

export const adminVariantsApi = {
  async getAll(params?: { isActive?: boolean }): Promise<VariantGroup[]> {
    const response = await api.get<VariantGroup[]>('/api/admin/variant-groups', { params });
    return response.data;
  },

  async getById(id: string): Promise<VariantGroup> {
    const response = await api.get<VariantGroup>(`/api/admin/variant-groups/${id}`);
    return response.data;
  },

  async create(data: {
    name: string;
    description?: string;
    displayType: string;
    isRequired?: boolean;
    allowMultiple?: boolean;
    displayOrder?: number;
  }): Promise<{ id: string }> {
    const response = await api.post<{ id: string }>('/api/admin/variant-groups', data);
    return response.data;
  },

  async update(id: string, data: {
    name: string;
    description?: string;
    displayType: string;
    isRequired: boolean;
    allowMultiple: boolean;
    displayOrder: number;
    isActive: boolean;
  }): Promise<void> {
    await api.put(`/api/admin/variant-groups/${id}`, data);
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/api/admin/variant-groups/${id}`);
  },

  async addOption(groupId: string, data: {
    name: string;
    code: string;
    hexColor?: string;
    imageUrl?: string;
    priceAdjustment?: number;
    isAvailable?: boolean;
    displayOrder?: number;
  }): Promise<{ id: string }> {
    const response = await api.post<{ id: string }>(`/api/admin/variant-groups/${groupId}/options`, data);
    return response.data;
  },

  async updateOption(groupId: string, optionId: string, data: {
    name: string;
    code: string;
    hexColor?: string;
    imageUrl?: string;
    priceAdjustment?: number;
    isAvailable: boolean;
    displayOrder: number;
  }): Promise<void> {
    await api.put(`/api/admin/variant-groups/${groupId}/options/${optionId}`, data);
  },

  async deleteOption(groupId: string, optionId: string): Promise<void> {
    await api.delete(`/api/admin/variant-groups/${groupId}/options/${optionId}`);
  },

  async getProductVariantGroups(productId: string): Promise<ProductVariantGroup[]> {
    const response = await api.get<ProductVariantGroup[]>(`/api/admin/variant-groups/product/${productId}`);
    return response.data;
  },

  async assignToProduct(productId: string, data: {
    variantGroupId: string;
    isRequired?: boolean;
    displayOrder?: number;
  }): Promise<{ id: string }> {
    const response = await api.post<{ id: string }>(`/api/admin/variant-groups/product/${productId}`, data);
    return response.data;
  },

  async removeFromProduct(productId: string, groupId: string): Promise<void> {
    await api.delete(`/api/admin/variant-groups/product/${productId}/${groupId}`);
  },
};

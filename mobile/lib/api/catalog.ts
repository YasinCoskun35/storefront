import { api } from '../api';
import type { Category, PagedResult, Product, ProductVariantGroup } from '../types';

export interface SearchProductsParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string;
}

export const catalogApi = {
  getCategories: () => api.get<Category[]>('/api/catalog/categories'),

  searchProducts: (params: SearchProductsParams = {}) =>
    api.get<PagedResult<Product>>('/api/catalog/products', {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 20,
        searchTerm: params.searchTerm || undefined,
        categoryId: params.categoryId || undefined,
      },
    }),

  getProduct: (id: string) => api.get<Product>(`/api/catalog/products/${id}`),

  getProductVariantGroups: (productId: string) =>
    api.get<ProductVariantGroup[]>(`/api/products/${productId}/variant-groups`),
};

import axios, { AxiosInstance } from "axios";

// Determine if we're running on the server or client
const isServer = typeof window === "undefined";

// API base URL - use container hostname on server, localhost on client
const API_BASE_URL = isServer
  ? process.env.API_URL || "http://localhost:8080"
  : "http://localhost:8080";

// Create axios instance
export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true, // Important for HttpOnly cookies
});

// Add JWT token from localStorage to all requests
api.interceptors.request.use(
  (config) => {
    // Only run on client side
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('accessToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Development: Add request/response logging
if (process.env.NODE_ENV === 'development') {
  // Request interceptor (for logging)
  /* eslint-disable no-console */
  api.interceptors.request.use(
    (config) => {
      const fullURL = `${config.baseURL}${config.url}`;
      console.log('🚀 API Request:', {
        method: config.method?.toUpperCase(),
        url: config.url,
        fullURL,
        data: config.data,
        params: config.params,
        headers: {
          Authorization: config.headers.Authorization ? '✅ Bearer token present' : '❌ No token',
        },
      });
      return config;
    },
    (error) => {
      console.error('❌ Request Error:', error);
      return Promise.reject(error);
    }
  );

  // Response interceptor
  api.interceptors.response.use(
    (response) => {
      console.log('✅ API Response:', {
        status: response.status,
        statusText: response.statusText,
        url: response.config.url,
        data: response.data,
      });
      return response;
    },
    (error) => {
      console.error('❌ API Error:', {
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        message: error.message,
        data: error.response?.data,
      });
      return Promise.reject(error);
    }
  );
  /* eslint-enable no-console */
}

// API Types
export interface Product {
  id: string;
  name: string;
  sku: string;
  description?: string;
  shortDescription?: string;
  /** Null when the platform operates in quote-request mode (Features.Pricing.Enabled = false) */
  price?: number | null;
  compareAtPrice?: number | null;
  productType?: string;
  stockStatus: number;
  quantity: number;
  categoryId: string;
  categoryName: string;
  brandId?: string;
  brandName?: string;
  isActive: boolean;
  isFeatured: boolean;
  primaryImageUrl?: string;
  slug?: string;
  createdAt: string;
}

export interface ProductDetail extends Product {
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  dimensionUnit?: string;
  weightUnit?: string;
  images: ProductImage[];
}

export interface ProductImage {
  id: string;
  url: string;
  type: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  slug?: string;
  imageUrl?: string;
  parentId?: string;
  displayOrder: number;
  isActive: boolean;
  showInNavbar?: boolean;
  productCount: number;
  /** Populated when fetching nested/hierarchical category tree for mega menu */
  children?: Category[];
}

export interface Brand {
  id: string;
  name: string;
  description?: string;
  logoUrl?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface BlogPostSummary {
  id: string;
  title: string;
  slug: string;
  summary?: string;
  featuredImage?: string;
  author?: string;
  publishedAt?: string;
  viewCount: number;
  category?: string;
}

export interface BlogPostDetail {
  id: string;
  title: string;
  slug: string;
  summary?: string;
  body: string;
  featuredImage?: string;
  author?: string;
  isPublished: boolean;
  publishedAt?: string;
  viewCount: number;
  tags?: string;
  category?: string;
  createdAt: string;
}

export interface StaticPage {
  id: string;
  title: string;
  slug: string;
  body: string;
  isPublished: boolean;
}

// Product Create/Update Types
export interface CreateProductDto {
  name: string;
  sku: string;
  description?: string;
  shortDescription?: string;
  price?: number;
  compareAtPrice?: number;
  stockStatus?: string;
  quantity?: number;
  categoryId: string;
  brandId?: string;
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  isActive?: boolean;
  isFeatured?: boolean;
  productType?: string;
  canBeSoldSeparately?: boolean;
}

// Category Create/Update Types
export interface CreateCategoryDto {
  name: string;
  description?: string;
  slug?: string;
  parentId?: string;
  displayOrder?: number;
  isActive?: boolean;
  showInNavbar?: boolean;
}

// API Client Methods
export const catalogApi = {
  searchProducts: async (params: {
    searchTerm?: string;
    categoryId?: string;
    brandId?: string;
    minPrice?: number;
    maxPrice?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedResult<Product>> => {
    const response = await api.get("/api/catalog/products", { params });
    return response.data;
  },

  getProductById: async (id: string): Promise<ProductDetail> => {
    const response = await api.get(`/api/catalog/products/${id}`);
    return response.data;
  },

  createProduct: async (product: CreateProductDto): Promise<{ id: string }> => {
    const response = await api.post("/api/catalog/products", product);
    return response.data;
  },

  updateProduct: async (id: string, product: {
    name: string;
    sku: string;
    description?: string;
    shortDescription?: string;
    categoryId: string;
    weight?: number;
    length?: number;
    width?: number;
    height?: number;
    isActive: boolean;
    isFeatured: boolean;
  }): Promise<void> => {
    await api.put(`/api/catalog/products/${id}`, product);
  },

  deleteProduct: async (id: string): Promise<void> => {
    await api.delete(`/api/catalog/products/${id}`);
  },

  uploadProductImage: async (
    productId: string,
    file: File,
    isPrimary: boolean = false
  ): Promise<{ message: string; fileName: string }> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await api.post(
      `/api/catalog/products/${productId}/images?isPrimary=${isPrimary}`,
      formData
    );
    return response.data;
  },

  getCategories: async (params?: { parentId?: string; showInNavbar?: boolean; all?: boolean }): Promise<Category[]> => {
    const response = await api.get("/api/catalog/categories", {
      params: params ?? {},
    });
    return response.data;
  },

  createCategory: async (category: CreateCategoryDto): Promise<{ id: string }> => {
    const response = await api.post("/api/catalog/categories", category);
    return response.data;
  },

  updateCategory: async (id: string, category: CreateCategoryDto): Promise<void> => {
    await api.put(`/api/catalog/categories/${id}`, category);
  },

  deleteCategory: async (id: string): Promise<void> => {
    await api.delete(`/api/catalog/categories/${id}`);
  },

  /** Returns a nested category tree built client-side from the flat list */
  getCategoriesTree: async (): Promise<Category[]> => {
    const response = await api.get<Category[]>("/api/catalog/categories", { params: { all: true } });
    const all: Category[] = response.data;
    const map = new Map<string, Category>();
    all.forEach((c) => map.set(c.id, { ...c, children: [] }));
    const roots: Category[] = [];
    map.forEach((cat) => {
      if (cat.parentId) {
        map.get(cat.parentId)?.children?.push(cat);
      } else {
        roots.push(cat);
      }
    });
    return roots;
  },
};

export interface HeroSlideDto {
  id: string;
  title: string;
  subtitle?: string;
  imageUrl: string;
  link: string;
  linkText: string;
}

export interface CategorySlideDto {
  id: string;
  name: string;
  slug: string;
  imageUrl: string;
  link: string;
  productCount: number;
}

export interface FeaturedBrandDto {
  id: string;
  name: string;
  logoUrl?: string;
  link: string;
}

export interface HomeSlidersData {
  heroSlides: HeroSlideDto[];
  categorySlides: CategorySlideDto[];
  featuredBrands: FeaturedBrandDto[];
}

export const contentApi = {
  getHomeSliders: async (): Promise<HomeSlidersData> => {
    const response = await api.get("/api/content/home-sliders");
    return response.data;
  },

  getBlogPosts: async (params: {
    category?: string;
    tag?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedResult<BlogPostSummary>> => {
    const response = await api.get("/api/content/blog", { params });
    return response.data;
  },

  getPageBySlug: async (slug: string): Promise<StaticPage> => {
    const response = await api.get(`/api/content/pages/${slug}`);
    return response.data;
  },
};

export const authApi = {
  login: async (email: string, password: string) => {
    const response = await api.post("/api/identity/auth/login", {
      email,
      password,
    });
    return response.data;
  },

  refresh: async (refreshToken: string) => {
    const response = await api.post("/api/identity/auth/refresh", {
      refreshToken,
    });
    return response.data;
  },
};


// API Client for Partner Management
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api';

export interface PartnerCompany {
  id: string;
  companyName: string;
  taxId: string;
  email: string;
  phone: string;
  city: string;
  state: string;
  country: string;
  status: 'Pending' | 'Active' | 'Suspended' | 'Rejected';
  userCount: number;
  createdAt: string;
  approvedAt: string | null;
}

export interface PartnerCompanyDetails extends PartnerCompany {
  address: string;
  postalCode: string;
  industry: string | null;
  website: string | null;
  employeeCount: number | null;
  annualRevenue: number | null;
  notes: string | null;
  approvedBy: string | null;
  approvalNotes: string | null;
  users: PartnerUser[];
  contacts: PartnerContact[];
}

export interface PartnerUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'User' | 'CompanyAdmin';
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface PartnerContact {
  id: string;
  name: string;
  title: string;
  email: string;
  phone: string;
  isPrimary: boolean;
}

export interface PartnerListResponse {
  items: PartnerCompany[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RegisterPartnerData {
  companyName: string;
  taxId: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  industry?: string;
  website?: string;
  employeeCount?: number;
  annualRevenue?: number;
  adminUser: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
  };
}

export interface PartnerLoginData {
  email: string;
  password: string;
}

export interface PartnerLoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    company: {
      id: string;
      name: string;
      status: string;
    };
  };
}

// Admin APIs
export const partnerAdminApi = {
  getPartners: async (
    searchTerm?: string,
    status?: string,
    pageNumber = 1,
    pageSize = 20,
    token?: string
  ): Promise<PartnerListResponse> => {
    const params = new URLSearchParams();
    if (searchTerm) params.append('searchTerm', searchTerm);
    if (status) params.append('status', status);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await axios.get(`${API_URL}/admin/partners?${params}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    return response.data;
  },

  getPartnerDetails: async (id: string, token?: string): Promise<PartnerCompanyDetails> => {
    const response = await axios.get(`${API_URL}/admin/partners/${id}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    return response.data;
  },

  createPartner: async (data: RegisterPartnerData, token?: string): Promise<{ id: string }> => {
    const response = await axios.post(`${API_URL}/admin/partners`, data, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    return response.data;
  },

  approvePartner: async (id: string, approvalNotes: string | null, token?: string): Promise<void> => {
    await axios.put(
      `${API_URL}/admin/partners/${id}/approve`,
      { approvalNotes },
      {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      }
    );
  },

  suspendPartner: async (id: string, reason: string | null, token?: string): Promise<void> => {
    await axios.put(
      `${API_URL}/admin/partners/${id}/suspend`,
      { reason },
      {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      }
    );
  },
};

// Partner Public APIs
export const partnerPublicApi = {
  login: async (data: PartnerLoginData): Promise<PartnerLoginResponse> => {
    const response = await axios.post(`${API_URL}/partners/auth/login`, data);
    return response.data;
  },
};

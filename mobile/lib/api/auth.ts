import { api } from '../api';
import type { AdminLoginResponse, LoginResponse } from '../types';

export const authApi = {
  login: (email: string, password: string) =>
    api.post<LoginResponse>('/api/identity/partners/auth/login', { email, password }),

  adminLogin: (email: string, password: string) =>
    api.post<AdminLoginResponse>('/api/identity/auth/login', { email, password }),
};

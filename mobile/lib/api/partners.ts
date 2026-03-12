import { api } from '../api';
import type { PartnerAccount, PartnerProfile } from '../types';

export const partnersApi = {
  getProfile: () => api.get<PartnerProfile>('/api/identity/partners/profile'),
  getAccount: () => api.get<PartnerAccount>('/api/identity/partners/account'),
};

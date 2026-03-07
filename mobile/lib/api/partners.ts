import { api } from '../api';
import type { PartnerProfile } from '../types';

export const partnersApi = {
  getProfile: () => api.get<PartnerProfile>('/api/identity/partners/profile'),
};

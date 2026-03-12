import { api } from '../api';

export interface AppSetting {
  key: string;
  value: string;
  displayName: string;
  description: string | null;
  category: string;
  dataType: string;
  updatedAt: string;
  updatedBy: string | null;
}

export interface SettingsGroup {
  category: string;
  settings: AppSetting[];
}

export const settingsApi = {
  async getAll(): Promise<AppSetting[]> {
    const response = await api.get('/api/content/settings');
    return response.data;
  },

  async getByCategory(category: string): Promise<AppSetting[]> {
    const response = await api.get(`/api/content/settings?category=${category}`);
    return response.data;
  },

  async update(key: string, value: string): Promise<void> {
    await api.put(`/api/content/settings/${key}`, { value });
  },

  // Helper function to check if a feature is enabled
  async isFeatureEnabled(featureKey: string): Promise<boolean> {
    try {
      const settings = await this.getAll();
      const setting = settings.find(s => s.key === featureKey);
      return setting?.value?.toLowerCase() === 'true';
    } catch {
      return false;
    }
  },

  // Helper function to get setting value
  async getValue(key: string): Promise<string | null> {
    try {
      const settings = await this.getAll();
      const setting = settings.find(s => s.key === key);
      return setting?.value || null;
    } catch {
      return null;
    }
  },
};

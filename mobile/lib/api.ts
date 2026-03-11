import axios from 'axios';
import Constants from 'expo-constants';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

/**
 * API base URL. Resolved in this order:
 * 1. EXPO_PUBLIC_API_URL env var (if set)
 * 2. Physical device: Use dev machine's IP from Expo (same host as Metro bundler)
 * 3. Android Emulator: 10.0.2.2 (host machine's localhost)
 * 4. iOS Simulator: localhost
 */
function getDefaultApiUrl(): string {
  // Physical device: Expo injects the dev machine's IP (e.g. 192.168.1.100:8081)
  const hostUri =
    Constants.expoConfig?.hostUri ??
    Constants.expoGoConfig?.debuggerHost ??
    (Constants.manifest as { debuggerHost?: string })?.debuggerHost;
  if (hostUri) {
    const host = hostUri.split(':')[0];
    if (host && host !== 'localhost' && host !== '127.0.0.1') {
      return `http://${host}:8080`;
    }
  }

  // Simulators/emulators
  if (Platform.OS === 'android') {
    return 'http://10.0.2.2:8080'; // Android emulator -> host machine
  }
  return 'http://localhost:8080'; // iOS simulator
}

export const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? getDefaultApiUrl();

export const TOKEN_KEY = 'partner_access_token';
export const USER_KEY = 'partner_user';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

// Attach token to every request
api.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync(TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  console.log(`[API] ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`, config.data ?? '');
  return config;
});

// On 401 or network errors clear token and redirect
let _onUnauthorized: (() => void) | null = null;
let _onNetworkError: (() => void) | null = null;

export function registerUnauthorizedHandler(handler: () => void) {
  _onUnauthorized = handler;
}

export function registerNetworkErrorHandler(handler: () => void) {
  _onNetworkError = handler;
}

api.interceptors.response.use(
  (response) => {
    console.log(`[API] ✓ ${response.status} ${response.config.url}`, response.data);
    return response;
  },
  async (error) => {
    const status = error.response?.status;
    const isTimeout = error.code === 'ECONNABORTED' || error.message?.includes('timeout');
    const isNetworkError = !error.response && !isTimeout;

    console.log(`[API] ✗ ${status} ${error.config?.url}`, error.response?.data ?? error.message);

    if (status === 401) {
      await SecureStore.deleteItemAsync(TOKEN_KEY);
      await SecureStore.deleteItemAsync(USER_KEY);
      _onUnauthorized?.();
    } else if (isTimeout || isNetworkError) {
      // Server unreachable — notify so the UI can show a proper error state
      _onNetworkError?.();
    }

    return Promise.reject(error);
  }
);

export function getImageUrl(path: string | null | undefined): string {
  if (!path) return '';
  if (path.startsWith('http')) return path;
  return `${API_BASE_URL}${path}`;
}

import axios from 'axios';
import { router } from 'expo-router';
import * as SecureStore from 'expo-secure-store';
import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { API_BASE_URL, TOKEN_KEY, USER_KEY, registerNetworkErrorHandler, registerUnauthorizedHandler } from './api';
import type { User } from './types';

interface AuthState {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  networkError: boolean;
  signIn: (token: string, user: User) => Promise<void>;
  signOut: () => Promise<void>;
  retryConnection: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [networkError, setNetworkError] = useState(false);
  const bootstrapRef = useRef(false);

  const signOut = useCallback(async () => {
    await SecureStore.deleteItemAsync(TOKEN_KEY);
    await SecureStore.deleteItemAsync(USER_KEY);
    setUser(null);
    setNetworkError(false);
    router.replace('/(auth)/login');
  }, []);

  const bootstrap = useCallback(async () => {
    setIsLoading(true);
    setNetworkError(false);
    try {
      const [token, stored] = await Promise.all([
        SecureStore.getItemAsync(TOKEN_KEY),
        SecureStore.getItemAsync(USER_KEY),
      ]);

      if (!token || !stored) {
        setIsLoading(false);
        return;
      }

      // Validate token with a short timeout — don't trust stale SecureStore data
      try {
        const response = await axios.get(`${API_BASE_URL}/api/identity/partners/profile`, {
          headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
          timeout: 5000,
        });
        const profile = response.data;
        const freshUser: User = {
          id: profile.id,
          email: profile.email,
          firstName: profile.firstName,
          lastName: profile.lastName,
          role: profile.role,
          discountRate: profile.discountRate,
          company: profile.company,
        };
        await SecureStore.setItemAsync(USER_KEY, JSON.stringify(freshUser));
        setUser(freshUser);
      } catch (err: any) {
        const status = err.response?.status;
        const noResponse = !err.response;

        if (status === 401 || status === 403 || status === 404) {
          // Token invalid or user deleted — force logout
          await SecureStore.deleteItemAsync(TOKEN_KEY);
          await SecureStore.deleteItemAsync(USER_KEY);
        } else if (noResponse) {
          // Server unreachable — keep stored user but flag network error
          setUser(JSON.parse(stored) as User);
          setNetworkError(true);
        } else {
          // 5xx or other — keep stored user, let individual screens handle errors
          setUser(JSON.parse(stored) as User);
        }
      }
    } catch {
      // SecureStore failure — treat as logged out
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!bootstrapRef.current) {
      bootstrapRef.current = true;
      bootstrap();
    }
  }, [bootstrap]);

  useEffect(() => {
    registerUnauthorizedHandler(signOut);
    registerNetworkErrorHandler(() => setNetworkError(true));
  }, [signOut]);

  const signIn = useCallback(async (token: string, userData: User) => {
    await SecureStore.setItemAsync(TOKEN_KEY, token);
    await SecureStore.setItemAsync(USER_KEY, JSON.stringify(userData));
    setUser(userData);
    setNetworkError(false);
  }, []);

  const retryConnection = useCallback(() => {
    bootstrap();
  }, [bootstrap]);

  return (
    <AuthContext.Provider
      value={{ user, isLoading, isAuthenticated: !!user, networkError, signIn, signOut, retryConnection }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Stack, useRouter, useSegments } from 'expo-router';
import React, { useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator } from 'react-native';
import { AuthProvider, useAuth } from '../lib/auth';
import { Colors } from '../constants/colors';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1 },
  },
});

function NetworkErrorScreen({ onRetry }: { onRetry: () => void }) {
  return (
    <View style={styles.errorContainer}>
      <Text style={styles.errorIcon}>📡</Text>
      <Text style={styles.errorTitle}>Cannot connect to server</Text>
      <Text style={styles.errorMessage}>
        Make sure the server is running and you are on the same network.
      </Text>
      <TouchableOpacity style={styles.retryButton} onPress={onRetry} activeOpacity={0.8}>
        <Text style={styles.retryText}>Retry</Text>
      </TouchableOpacity>
    </View>
  );
}

function NavigationGuard() {
  const { user, isAuthenticated, isLoading, networkError, retryConnection } = useAuth();
  const segments = useSegments();
  const router = useRouter();

  useEffect(() => {
    if (isLoading) return;
    if (networkError) return; // stay on error screen, don't redirect

    const inAuth = segments[0] === '(auth)';
    const inAdmin = segments[0] === '(admin)';
    const inTabs = segments[0] === '(tabs)';

    if (!isAuthenticated && !inAuth) {
      router.replace('/(auth)/login');
    } else if (isAuthenticated && inAuth) {
      router.replace(user?.isAdmin ? '/(admin)' : '/(tabs)');
    } else if (isAuthenticated && user?.isAdmin && inTabs) {
      router.replace('/(admin)');
    } else if (isAuthenticated && !user?.isAdmin && inAdmin) {
      router.replace('/(tabs)');
    }
  }, [isAuthenticated, isLoading, networkError, segments, router, user]);

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Connecting...</Text>
      </View>
    );
  }

  if (networkError) {
    return <NetworkErrorScreen onRetry={retryConnection} />;
  }

  return (
    <Stack
      screenOptions={{
        headerStyle: { backgroundColor: Colors.surface },
        headerTitleStyle: { fontWeight: '700', fontSize: 17 },
        headerShadowVisible: false,
        headerTintColor: Colors.primary,
        contentStyle: { backgroundColor: Colors.background },
      }}
    >
      <Stack.Screen name="(auth)" options={{ headerShown: false }} />
      <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      <Stack.Screen name="(admin)" options={{ headerShown: false }} />
      <Stack.Screen
        name="checkout"
        options={{ title: 'Checkout', headerBackTitle: 'Cart' }}
      />
    </Stack>
  );
}

export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <NavigationGuard />
      </AuthProvider>
    </QueryClientProvider>
  );
}

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.background,
    gap: 12,
  },
  loadingText: {
    fontSize: 14,
    color: Colors.textMuted,
  },
  errorContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.background,
    paddingHorizontal: 32,
    gap: 12,
  },
  errorIcon: {
    fontSize: 48,
    marginBottom: 8,
  },
  errorTitle: {
    fontSize: 20,
    fontWeight: '700',
    color: Colors.text,
    textAlign: 'center',
  },
  errorMessage: {
    fontSize: 14,
    color: Colors.textMuted,
    textAlign: 'center',
    lineHeight: 22,
  },
  retryButton: {
    marginTop: 8,
    backgroundColor: Colors.primary,
    paddingHorizontal: 32,
    paddingVertical: 12,
    borderRadius: 8,
  },
  retryText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '700',
  },
});

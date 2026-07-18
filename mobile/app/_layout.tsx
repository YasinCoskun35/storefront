import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as Notifications from 'expo-notifications';
import { Stack, useRouter, useSegments } from 'expo-router';
import React, { useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { ActivityIndicator, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '../constants/colors';
import { AuthProvider, useAuth } from '../lib/auth';
import '../lib/i18n';

// ── Root Error Boundary ────────────────────────────────────────────────────────
// Catches uncaught JS errors that would otherwise show a blank white screen.
interface ErrorBoundaryState { hasError: boolean; message: string }

class RootErrorBoundary extends React.Component<
  { children: React.ReactNode },
  ErrorBoundaryState
> {
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { hasError: false, message: '' };
  }

  static getDerivedStateFromError(error: unknown): ErrorBoundaryState {
    return {
      hasError: true,
      message: error instanceof Error ? error.message : 'An unexpected error occurred.',
    };
  }

  override render() {
    if (this.state.hasError) {
      return (
        <View style={errorStyles.container}>
          <Text style={errorStyles.icon}>⚠️</Text>
          <Text style={errorStyles.title}>Something went wrong</Text>
          <Text style={errorStyles.message}>{this.state.message}</Text>
          <TouchableOpacity
            style={errorStyles.button}
            onPress={() => this.setState({ hasError: false, message: '' })}
            activeOpacity={0.8}
          >
            <Text style={errorStyles.buttonText}>Try Again</Text>
          </TouchableOpacity>
        </View>
      );
    }
    return this.props.children;
  }
}

const errorStyles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.background,
    paddingHorizontal: 32,
    gap: 12,
  },
  icon:       { fontSize: 48, marginBottom: 8 },
  title:      { fontSize: 20, fontWeight: '700', color: Colors.text, textAlign: 'center' },
  message:    { fontSize: 13, color: Colors.textMuted, textAlign: 'center', lineHeight: 20 },
  button:     { marginTop: 8, backgroundColor: Colors.primary, paddingHorizontal: 32, paddingVertical: 12, borderRadius: 8 },
  buttonText: { color: '#fff', fontSize: 15, fontWeight: '700' },
});

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1 },
  },
});

function NetworkErrorScreen({ onRetry }: { onRetry: () => void }) {
  const { t } = useTranslation();
  return (
    <View style={styles.errorContainer}>
      <Text style={styles.errorIcon}>📡</Text>
      <Text style={styles.errorTitle}>{t('network.cannotConnect')}</Text>
      <Text style={styles.errorMessage}>
        {t('network.checkServer')}
      </Text>
      <TouchableOpacity style={styles.retryButton} onPress={onRetry} activeOpacity={0.8}>
        <Text style={styles.retryText}>{t('network.retry')}</Text>
      </TouchableOpacity>
    </View>
  );
}

function NavigationGuard() {
  const { t } = useTranslation();
  const { user, isAuthenticated, isLoading, networkError, retryConnection } = useAuth();
  const segments = useSegments();
  const router = useRouter();
  const notificationListener = useRef<Notifications.EventSubscription | null>(null);

  // Navigate to order detail when user taps a push notification
  useEffect(() => {
    notificationListener.current = Notifications.addNotificationResponseReceivedListener((response) => {
      const data = response.notification.request.content.data as { orderId?: string } | undefined;
      if (data?.orderId && isAuthenticated) {
        router.push(`/(tabs)/orders/${data.orderId}`);
      }
    });

    return () => {
      notificationListener.current?.remove();
    };
  }, [isAuthenticated, router]);

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
        <Text style={styles.loadingText}>{t('network.connecting')}</Text>
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
    <RootErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <NavigationGuard />
        </AuthProvider>
      </QueryClientProvider>
    </RootErrorBoundary>
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

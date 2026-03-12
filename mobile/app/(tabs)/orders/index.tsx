import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  FlatList,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { OrderCard } from '../../../components/orders/OrderCard';
import { EmptyState } from '../../../components/ui/EmptyState';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { ordersApi } from '../../../lib/api/orders';

const TABS = [
  { label: 'All', value: null },
  { label: 'Pending', value: 'Pending' },
  { label: 'Active', value: 'Active' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Cancelled', value: 'Cancelled' },
] as const;

type TabValue = typeof TABS[number]['value'];

export default function OrdersScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<TabValue>(null);

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['orders', { status: activeTab }],
    queryFn: () =>
      ordersApi
        .getOrders({ status: activeTab ?? undefined, pageSize: 50 })
        .then((r) => r.data),
  });

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={refetch} />;

  const orders = data?.items ?? [];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Filter Tabs */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.tabRow}
      >
        {TABS.map((tab) => (
          <TouchableOpacity
            key={String(tab.value)}
            style={[styles.tab, activeTab === tab.value && styles.tabActive]}
            onPress={() => setActiveTab(tab.value)}
          >
            <Text style={[styles.tabLabel, activeTab === tab.value && styles.tabLabelActive]}>
              {tab.label}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {orders.length === 0 ? (
        <EmptyState title={t('orders.noOrders')} subtitle={t('orders.startBrowsing')} />
      ) : (
        <FlatList
          data={orders}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={false} onRefresh={refetch} />}
          renderItem={({ item }) => (
            <OrderCard
              order={item}
              onPress={() => router.push(`/(tabs)/orders/${item.id}` as never)}
            />
          )}
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  tabRow: { paddingHorizontal: 12, paddingVertical: 10, gap: 8 },
  tab: {
    paddingHorizontal: 16,
    paddingVertical: 7,
    borderRadius: 16,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
  },
  tabActive: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  tabLabel: { fontSize: 13, color: Colors.textSecondary, fontWeight: '500' },
  tabLabelActive: { color: Colors.primary, fontWeight: '700' },
  list: { padding: 16, paddingBottom: 32 },
});

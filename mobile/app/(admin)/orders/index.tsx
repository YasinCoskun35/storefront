import { useQuery } from '@tanstack/react-query';
import { useLocalSearchParams, useRouter } from 'expo-router';
import React, { useState } from 'react';
import { FlatList, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Card } from '../../../components/ui/Card';
import { EmptyState } from '../../../components/ui/EmptyState';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { adminApi } from '../../../lib/api/admin';

const STATUS_FILTERS = ['All', 'Pending', 'Confirmed', 'InProduction', 'ReadyToShip', 'Shipping', 'Delivered', 'Completed', 'Cancelled'];

const STATUS_COLORS: Record<string, string> = {
  Pending: Colors.warning,
  QuoteSent: Colors.info,
  Confirmed: Colors.success,
  InProduction: Colors.purple,
  ReadyToShip: Colors.indigo,
  Shipping: Colors.orange,
  Delivered: Colors.success,
  Completed: Colors.gray,
  Cancelled: Colors.error,
};

export default function AdminOrdersScreen() {
  const router = useRouter();
  const params = useLocalSearchParams<{ status?: string }>();
  const [activeStatus, setActiveStatus] = useState(params.status ?? 'All');

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-orders', activeStatus],
    queryFn: () => adminApi.getOrders({
      status: activeStatus === 'All' ? undefined : activeStatus,
      pageSize: 50,
    }),
  });

  const orders = data?.data?.items ?? [];

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={refetch} />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Status filter chips */}
      <FlatList
        horizontal
        data={STATUS_FILTERS}
        keyExtractor={(s) => s}
        showsHorizontalScrollIndicator={false}
        style={styles.filterList}
        contentContainerStyle={styles.filterContent}
        renderItem={({ item }) => (
          <TouchableOpacity
            style={[styles.chip, activeStatus === item && styles.chipActive]}
            onPress={() => setActiveStatus(item)}
          >
            <Text style={[styles.chipText, activeStatus === item && styles.chipTextActive]}>
              {item}
            </Text>
          </TouchableOpacity>
        )}
      />

      <FlatList
        data={orders}
        keyExtractor={(o) => o.id}
        contentContainerStyle={styles.list}
        ListEmptyComponent={<EmptyState title="No orders found" />}
        renderItem={({ item }) => (
          <TouchableOpacity onPress={() => router.push(`/(admin)/orders/${item.id}` as never)}>
            <Card style={styles.orderCard}>
              <View style={styles.orderHeader}>
                <Text style={styles.orderNumber}>{item.orderNumber}</Text>
                <View style={[styles.badge, { backgroundColor: (STATUS_COLORS[item.status] ?? Colors.gray) + '20' }]}>
                  <Text style={[styles.badgeText, { color: STATUS_COLORS[item.status] ?? Colors.gray }]}>
                    {item.status}
                  </Text>
                </View>
              </View>
              <Text style={styles.company}>{item.partnerCompanyName}</Text>
              <View style={styles.meta}>
                <Text style={styles.metaText}>{item.itemCount} item{item.itemCount !== 1 ? 's' : ''}</Text>
                <Text style={styles.metaText}>{new Date(item.createdAt).toLocaleDateString()}</Text>
              </View>
            </Card>
          </TouchableOpacity>
        )}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  filterList: { maxHeight: 50, flexGrow: 0 },
  filterContent: { paddingHorizontal: 12, paddingVertical: 8, gap: 8 },
  chip: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 20,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  chipActive: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  chipText: { fontSize: 12, fontWeight: '600', color: Colors.textSecondary },
  chipTextActive: { color: '#fff' },
  list: { padding: 12, gap: 10, paddingBottom: 40 },
  orderCard: { padding: 14 },
  orderHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 },
  orderNumber: { fontSize: 14, fontWeight: '700', color: Colors.text },
  badge: { borderRadius: 6, paddingHorizontal: 8, paddingVertical: 2 },
  badgeText: { fontSize: 11, fontWeight: '700' },
  company: { fontSize: 13, color: Colors.textSecondary, marginBottom: 8 },
  meta: { flexDirection: 'row', justifyContent: 'space-between' },
  metaText: { fontSize: 12, color: Colors.textMuted },
});

import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React from 'react';
import { useTranslation } from 'react-i18next';
import {
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { OrderCard } from '../../components/orders/OrderCard';
import { Card } from '../../components/ui/Card';
import { ErrorState } from '../../components/ui/ErrorState';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { ordersApi } from '../../lib/api/orders';
import { useAuth } from '../../lib/auth';

interface StatCardProps {
  label: string;
  value: number;
  color: string;
  bg: string;
}

function StatCard({ label, value, color, bg }: StatCardProps) {
  return (
    <View style={[styles.statCard, { backgroundColor: bg }]}>
      <Text style={[styles.statValue, { color }]}>{value}</Text>
      <Text style={[styles.statLabel, { color }]}>{label}</Text>
    </View>
  );
}

interface QuickActionProps {
  icon: string;
  label: string;
  onPress: () => void;
}

function QuickAction({ icon, label, onPress }: QuickActionProps) {
  return (
    <TouchableOpacity style={styles.quickAction} onPress={onPress} activeOpacity={0.8}>
      <Text style={styles.quickActionIcon}>{icon}</Text>
      <Text style={styles.quickActionLabel}>{label}</Text>
    </TouchableOpacity>
  );
}

export default function DashboardScreen() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const router = useRouter();

  const {
    data: stats,
    isLoading: statsLoading,
    isError: statsError,
    refetch: refetchStats,
  } = useQuery({
    queryKey: ['order-stats'],
    queryFn: () => ordersApi.getOrderStats().then((r) => r.data),
  });

  const {
    data: recentOrders,
    isLoading: ordersLoading,
    isError: ordersError,
    refetch: refetchOrders,
  } = useQuery({
    queryKey: ['orders', { pageSize: 5 }],
    queryFn: () => ordersApi.getOrders({ pageSize: 5 }).then((r) => r.data),
  });

  const isLoading = statsLoading || ordersLoading;
  const isError = statsError || ordersError;

  function handleRefresh() {
    refetchStats();
    refetchOrders();
  }

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={handleRefresh} />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl refreshing={false} onRefresh={handleRefresh} />
        }
      >
        {/* Welcome */}
        <View style={styles.welcome}>
          <Text style={styles.welcomeText}>
            {t('dashboard.welcome')}, {user?.firstName} {user?.lastName}
          </Text>
          <Text style={styles.companyText}>{user?.company?.name}</Text>
        </View>

        {/* Stats */}
        {stats && (
          <View style={styles.statsGrid}>
            <StatCard label={t('dashboard.totalOrders')} value={stats.totalOrders} color={Colors.primary} bg={Colors.primaryLight} />
            <StatCard label={t('dashboard.pendingOrders')} value={stats.pendingOrders} color={Colors.statusPending} bg={Colors.statusPendingBg} />
            <StatCard label={t('dashboard.activeOrders')} value={stats.activeOrders} color={Colors.statusConfirmed} bg={Colors.statusConfirmedBg} />
            <StatCard label={t('dashboard.completedOrders')} value={stats.completedOrders} color={Colors.statusCompleted} bg={Colors.statusCompletedBg} />
          </View>
        )}

        {/* Quick Actions */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>{t('dashboard.quickActions')}</Text>
          <View style={styles.quickActions}>
            <QuickAction icon="📦" label={t('dashboard.browseCatalog')} onPress={() => router.push('/(tabs)/products/index')} />
            <QuickAction icon="🛒" label={t('dashboard.myCart')} onPress={() => router.push('/(tabs)/cart')} />
            <QuickAction icon="📋" label={t('dashboard.myOrders')} onPress={() => router.push('/(tabs)/orders/index')} />
          </View>
        </Card>

        {/* Recent Orders */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>{t('dashboard.recentOrders')}</Text>
            <TouchableOpacity onPress={() => router.push('/(tabs)/orders/index')}>
              <Text style={styles.viewAll}>{t('common.viewAll')}</Text>
            </TouchableOpacity>
          </View>
          {recentOrders?.items.length === 0 ? (
            <Text style={styles.emptyText}>{t('orders.noOrdersYet')}</Text>
          ) : (
            recentOrders?.items.map((order) => (
              <OrderCard
                key={order.id}
                order={order}
                onPress={() => router.push(`/(tabs)/orders/${order.id}` as never)}
              />
            ))
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { flex: 1 },
  content: { padding: 16, paddingBottom: 32 },
  welcome: { marginBottom: 20 },
  welcomeText: { fontSize: 20, fontWeight: '700', color: Colors.text },
  companyText: { fontSize: 14, color: Colors.textSecondary, marginTop: 2 },
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
    marginBottom: 16,
  },
  statCard: {
    flex: 1,
    minWidth: '44%',
    borderRadius: 12,
    padding: 16,
    alignItems: 'center',
  },
  statValue: { fontSize: 28, fontWeight: '800' },
  statLabel: { fontSize: 12, fontWeight: '600', marginTop: 2 },
  section: { marginBottom: 16 },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  sectionTitle: { fontSize: 15, fontWeight: '700', color: Colors.text, marginBottom: 12 },
  viewAll: { fontSize: 13, color: Colors.primary, fontWeight: '600' },
  quickActions: { flexDirection: 'row', gap: 10 },
  quickAction: {
    flex: 1,
    backgroundColor: Colors.primaryLight,
    borderRadius: 10,
    padding: 14,
    alignItems: 'center',
    gap: 6,
  },
  quickActionIcon: { fontSize: 22 },
  quickActionLabel: { fontSize: 11, fontWeight: '600', color: Colors.primary, textAlign: 'center' },
  emptyText: { fontSize: 14, color: Colors.textMuted, textAlign: 'center', paddingVertical: 20 },
});

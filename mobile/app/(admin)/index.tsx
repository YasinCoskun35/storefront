import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Card } from '../../components/ui/Card';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { adminApi } from '../../lib/api/admin';
import { useAuth } from '../../lib/auth';

function StatCard({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <Card style={[styles.statCard, { borderLeftColor: color, borderLeftWidth: 3 }]}>
      <Text style={[styles.statValue, { color }]}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </Card>
  );
}

export default function AdminDashboard() {
  const { t } = useTranslation();
  const { user, signOut } = useAuth();
  const router = useRouter();

  const { data: statsRes, isLoading } = useQuery({
    queryKey: ['admin-order-stats'],
    queryFn: adminApi.getOrderStats,
  });

  const { data: ordersRes } = useQuery({
    queryKey: ['admin-orders', 'Pending'],
    queryFn: () => adminApi.getOrders({ status: 'Pending', pageSize: 5 }),
  });

  const stats = statsRes?.data;
  const recentOrders = ordersRes?.data?.items ?? [];

  if (isLoading) return <LoadingScreen />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.greeting}>
          {t('dashboard.welcome')}, {user?.firstName} 👋
        </Text>

        {/* Stats */}
        <View style={styles.statsGrid}>
          <StatCard label={t('dashboard.totalOrders')} value={stats?.totalOrders ?? 0} color={Colors.primary} />
          <StatCard label={t('dashboard.pendingOrders')} value={stats?.pendingOrders ?? 0} color={Colors.warning} />
          <StatCard label={t('dashboard.activeOrders')} value={stats?.activeOrders ?? 0} color={Colors.success} />
          <StatCard label={t('dashboard.completedOrders')} value={stats?.completedOrders ?? 0} color={Colors.gray} />
        </View>

        {/* Quick actions */}
        <Text style={styles.sectionTitle}>{t('dashboard.quickActions')}</Text>
        <View style={styles.actions}>
          <TouchableOpacity
            style={styles.actionBtn}
            onPress={() => router.push('/(admin)/orders' as never)}
          >
            <Text style={styles.actionIcon}>📋</Text>
            <Text style={styles.actionLabel}>{t('nav.adminOrders')}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.actionBtn}
            onPress={() => router.push('/(admin)/partners' as never)}
          >
            <Text style={styles.actionIcon}>🏢</Text>
            <Text style={styles.actionLabel}>{t('nav.partners')}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.actionBtn, { backgroundColor: Colors.warningLight }]}
            onPress={() => router.push({ pathname: '/(admin)/orders', params: { status: 'Pending' } } as never)}
          >
            <Text style={styles.actionIcon}>⏳</Text>
            <Text style={styles.actionLabel}>{t('dashboard.pendingOrders')}</Text>
          </TouchableOpacity>
        </View>

        {/* Recent pending orders */}
        {recentOrders.length > 0 && (
          <>
            <Text style={styles.sectionTitle}>{t('dashboard.pendingOrders')}</Text>
            <Card>
              {recentOrders.map((order, i) => (
                <TouchableOpacity
                  key={order.id}
                  style={[styles.orderRow, i < recentOrders.length - 1 && styles.orderRowBorder]}
                  onPress={() => router.push(`/(admin)/orders/${order.id}` as never)}
                >
                  <View style={styles.orderInfo}>
                    <Text style={styles.orderNumber}>{order.orderNumber}</Text>
                    <Text style={styles.orderCompany}>{order.partnerCompanyName}</Text>
                  </View>
                  <Text style={styles.orderArrow}>›</Text>
                </TouchableOpacity>
              ))}
            </Card>
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },
  greeting: { fontSize: 20, fontWeight: '700', color: Colors.text, marginBottom: 16 },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 10, marginBottom: 20 },
  statCard: { flex: 1, minWidth: '45%', padding: 14 },
  statValue: { fontSize: 28, fontWeight: '800', marginBottom: 2 },
  statLabel: { fontSize: 12, color: Colors.textSecondary, fontWeight: '600' },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  actions: { flexDirection: 'row', gap: 10, marginBottom: 20 },
  actionBtn: {
    flex: 1,
    backgroundColor: Colors.primaryLight,
    borderRadius: 12,
    padding: 14,
    alignItems: 'center',
  },
  actionIcon: { fontSize: 24, marginBottom: 6 },
  actionLabel: { fontSize: 11, fontWeight: '700', color: Colors.primary },
  orderRow: { flexDirection: 'row', alignItems: 'center', paddingVertical: 12 },
  orderRowBorder: { borderBottomWidth: 1, borderBottomColor: Colors.borderLight },
  orderInfo: { flex: 1 },
  orderNumber: { fontSize: 13, fontWeight: '700', color: Colors.text },
  orderCompany: { fontSize: 12, color: Colors.textSecondary, marginTop: 2 },
  orderArrow: { fontSize: 20, color: Colors.textMuted },
});

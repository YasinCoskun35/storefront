import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocalSearchParams } from 'expo-router';
import React from 'react';
import { Alert, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Button } from '../../../components/ui/Button';
import { Card } from '../../../components/ui/Card';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { adminApi } from '../../../lib/api/admin';

const STATUS_COLORS: Record<string, string> = {
  Pending: Colors.warning,
  Active: Colors.success,
  Suspended: Colors.error,
  Rejected: Colors.gray,
};

function Row({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <View style={styles.row}>
      <Text style={styles.rowLabel}>{label}</Text>
      <Text style={styles.rowValue}>{value || '—'}</Text>
    </View>
  );
}

export default function AdminPartnerDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const queryClient = useQueryClient();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-partner', id],
    queryFn: () => adminApi.getPartnerDetails(id),
  });

  const approveMutation = useMutation({
    mutationFn: () => adminApi.approvePartner(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
    },
    onError: (err: any) => Alert.alert('Error', err.response?.data?.message ?? 'Failed'),
  });

  const suspendMutation = useMutation({
    mutationFn: () => adminApi.suspendPartner(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-partners'] });
    },
    onError: (err: any) => Alert.alert('Error', err.response?.data?.message ?? 'Failed'),
  });

  if (isLoading) return <LoadingScreen />;
  if (isError || !data?.data) return <ErrorState onRetry={refetch} />;

  const partner = data.data;
  const statusColor = STATUS_COLORS[partner.status] ?? Colors.gray;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>

        {/* Header */}
        <Card style={styles.section}>
          <View style={styles.header}>
            <Text style={styles.companyName}>{partner.companyName}</Text>
            <View style={[styles.badge, { backgroundColor: statusColor + '20' }]}>
              <Text style={[styles.badgeText, { color: statusColor }]}>{partner.status}</Text>
            </View>
          </View>
          <Text style={styles.taxId}>Tax ID: {partner.taxId}</Text>
        </Card>

        {/* Actions */}
        <View style={styles.actions}>
          {partner.status === 'Pending' && (
            <Button
              title="Approve"
              onPress={() =>
                Alert.alert('Approve Partner', `Approve ${partner.companyName}?`, [
                  { text: 'Cancel', style: 'cancel' },
                  { text: 'Approve', onPress: () => approveMutation.mutate() },
                ])
              }
              loading={approveMutation.isPending}
              style={styles.actionBtn}
            />
          )}
          {partner.status === 'Active' && (
            <Button
              title="Suspend"
              variant="danger"
              onPress={() =>
                Alert.alert('Suspend Partner', `Suspend ${partner.companyName}?`, [
                  { text: 'Cancel', style: 'cancel' },
                  { text: 'Suspend', style: 'destructive', onPress: () => suspendMutation.mutate() },
                ])
              }
              loading={suspendMutation.isPending}
              style={styles.actionBtn}
            />
          )}
        </View>

        {/* Contact Info */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Contact</Text>
          <Row label="Email" value={partner.email} />
          <Row label="Phone" value={partner.phone} />
          <Row label="City" value={`${partner.city}, ${partner.state}`} />
          <Row label="Country" value={partner.country} />
        </Card>

        {/* Users */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Users ({partner.users?.length ?? 0})</Text>
          {(partner.users ?? []).length === 0 ? (
            <Text style={styles.emptyText}>No users.</Text>
          ) : (
            partner.users?.map((u, i) => (
              <View key={u.id} style={[styles.userRow, i < (partner.users?.length ?? 0) - 1 && styles.userBorder]}>
                <View style={styles.userInfo}>
                  <Text style={styles.userName}>{u.firstName} {u.lastName}</Text>
                  <Text style={styles.userEmail}>{u.email}</Text>
                </View>
                <View>
                  <Text style={[styles.userRole, { color: u.isActive ? Colors.success : Colors.error }]}>
                    {u.role}
                  </Text>
                  <Text style={styles.userStatus}>{u.isActive ? 'Active' : 'Inactive'}</Text>
                </View>
              </View>
            ))
          )}
        </Card>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },
  section: { marginBottom: 16 },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 4 },
  companyName: { fontSize: 18, fontWeight: '800', color: Colors.text, flex: 1, marginRight: 8 },
  badge: { borderRadius: 6, paddingHorizontal: 10, paddingVertical: 3 },
  badgeText: { fontSize: 12, fontWeight: '700' },
  taxId: { fontSize: 12, color: Colors.textMuted },
  actions: { flexDirection: 'row', gap: 10, marginBottom: 16 },
  actionBtn: { flex: 1 },
  row: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 8, borderBottomWidth: 1, borderBottomColor: Colors.borderLight },
  rowLabel: { fontSize: 13, color: Colors.textSecondary, flex: 1 },
  rowValue: { fontSize: 13, color: Colors.text, fontWeight: '500', flex: 2, textAlign: 'right' },
  userRow: { flexDirection: 'row', alignItems: 'center', paddingVertical: 10 },
  userBorder: { borderBottomWidth: 1, borderBottomColor: Colors.borderLight },
  userInfo: { flex: 1 },
  userName: { fontSize: 13, fontWeight: '700', color: Colors.text },
  userEmail: { fontSize: 11, color: Colors.textSecondary, marginTop: 1 },
  userRole: { fontSize: 12, fontWeight: '700', textAlign: 'right' },
  userStatus: { fontSize: 11, color: Colors.textMuted, textAlign: 'right' },
  emptyText: { fontSize: 13, color: Colors.textMuted, textAlign: 'center', paddingVertical: 8 },
});

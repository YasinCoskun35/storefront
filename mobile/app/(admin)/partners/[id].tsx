import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocalSearchParams } from 'expo-router';
import React, { useState } from 'react';
import { Alert, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
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

const TRANSACTION_TYPES = ['OrderDebit', 'PaymentCredit', 'ManualAdjustment'];
const PAYMENT_METHODS = ['Cash', 'Check', 'PromissoryNote', 'BankTransfer'];

export default function AdminPartnerDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const queryClient = useQueryClient();

  const [editingRate, setEditingRate] = useState(false);
  const [discountRateInput, setDiscountRateInput] = useState('');
  const [showTxForm, setShowTxForm] = useState(false);
  const [txType, setTxType] = useState('PaymentCredit');
  const [txAmount, setTxAmount] = useState('');
  const [txPaymentMethod, setTxPaymentMethod] = useState('Cash');
  const [txReference, setTxReference] = useState('');
  const [txNotes, setTxNotes] = useState('');

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

  const updatePricingMutation = useMutation({
    mutationFn: (rate: number) => adminApi.updatePartnerPricing(id, rate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      setEditingRate(false);
    },
    onError: (err: any) => Alert.alert('Error', err.response?.data?.message ?? 'Failed to update pricing'),
  });

  const recordTransactionMutation = useMutation({
    mutationFn: () =>
      adminApi.recordPartnerTransaction(id, {
        type: txType,
        amount: parseFloat(txAmount),
        paymentMethod: txType === 'PaymentCredit' ? txPaymentMethod : undefined,
        orderReference: txReference.trim() || undefined,
        notes: txNotes.trim() || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-partner', id] });
      setShowTxForm(false);
      setTxAmount('');
      setTxReference('');
      setTxNotes('');
    },
    onError: (err: any) => Alert.alert('Error', err.response?.data?.message ?? 'Failed to record transaction'),
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

        {/* Pricing Policy */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Pricing Policy</Text>
          {editingRate ? (
            <View>
              <Text style={styles.rowLabel}>Discount Rate (%)</Text>
              <TextInput
                style={styles.input}
                value={discountRateInput}
                onChangeText={setDiscountRateInput}
                keyboardType="decimal-pad"
                placeholder="0 – 100"
                placeholderTextColor={Colors.textMuted}
              />
              <View style={styles.inlineActions}>
                <TouchableOpacity
                  style={[styles.inlineBtn, styles.inlineBtnPrimary]}
                  onPress={() => {
                    const rate = parseFloat(discountRateInput);
                    if (isNaN(rate) || rate < 0 || rate > 100) {
                      Alert.alert('Invalid', 'Enter a value between 0 and 100');
                      return;
                    }
                    updatePricingMutation.mutate(rate);
                  }}
                >
                  <Text style={styles.inlineBtnPrimaryText}>Save</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.inlineBtn, styles.inlineBtnSecondary]}
                  onPress={() => setEditingRate(false)}
                >
                  <Text style={styles.inlineBtnSecondaryText}>Cancel</Text>
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <View style={styles.pricingRow}>
              <View>
                <Text style={styles.rowLabel}>Partner Discount</Text>
                <Text style={styles.pricingValue}>{partner.discountRate ?? 0}%</Text>
              </View>
              <TouchableOpacity
                style={styles.editBtn}
                onPress={() => {
                  setDiscountRateInput(String(partner.discountRate ?? 0));
                  setEditingRate(true);
                }}
              >
                <Text style={styles.editBtnText}>Edit</Text>
              </TouchableOpacity>
            </View>
          )}
        </Card>

        {/* Current Account */}
        <Card style={styles.section}>
          <View style={styles.accountHeader}>
            <Text style={styles.sectionTitle}>Current Account</Text>
            <Text style={[styles.balanceValue, { color: (partner.currentBalance ?? 0) >= 0 ? Colors.error : Colors.success }]}>
              ₺{(partner.currentBalance ?? 0).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
            </Text>
          </View>
          <Text style={styles.balanceHint}>Positive = owes us</Text>

          {/* Record Transaction */}
          {showTxForm ? (
            <View style={styles.txForm}>
              <Text style={styles.txFormTitle}>Record Transaction</Text>
              <Text style={styles.txLabel}>Type</Text>
              <View style={styles.typeChips}>
                {TRANSACTION_TYPES.map((t) => (
                  <TouchableOpacity
                    key={t}
                    style={[styles.typeChip, txType === t && styles.typeChipActive]}
                    onPress={() => setTxType(t)}
                  >
                    <Text style={[styles.typeChipText, txType === t && styles.typeChipTextActive]}>{t}</Text>
                  </TouchableOpacity>
                ))}
              </View>
              {txType === 'PaymentCredit' && (
                <>
                  <Text style={styles.txLabel}>Payment Method</Text>
                  <View style={styles.typeChips}>
                    {PAYMENT_METHODS.map((m) => (
                      <TouchableOpacity
                        key={m}
                        style={[styles.typeChip, txPaymentMethod === m && styles.typeChipActive]}
                        onPress={() => setTxPaymentMethod(m)}
                      >
                        <Text style={[styles.typeChipText, txPaymentMethod === m && styles.typeChipTextActive]}>{m}</Text>
                      </TouchableOpacity>
                    ))}
                  </View>
                </>
              )}
              <Text style={styles.txLabel}>Amount (₺)</Text>
              <TextInput
                style={styles.input}
                value={txAmount}
                onChangeText={setTxAmount}
                keyboardType="decimal-pad"
                placeholder="0.00"
                placeholderTextColor={Colors.textMuted}
              />
              <Text style={styles.txLabel}>Order Reference (optional)</Text>
              <TextInput
                style={styles.input}
                value={txReference}
                onChangeText={setTxReference}
                placeholder="e.g. ORD-2024-001"
                placeholderTextColor={Colors.textMuted}
              />
              <Text style={styles.txLabel}>Notes (optional)</Text>
              <TextInput
                style={[styles.input, styles.inputMultiline]}
                value={txNotes}
                onChangeText={setTxNotes}
                placeholder="Additional notes..."
                placeholderTextColor={Colors.textMuted}
                multiline
                numberOfLines={2}
              />
              <View style={styles.inlineActions}>
                <TouchableOpacity
                  style={[styles.inlineBtn, styles.inlineBtnPrimary]}
                  onPress={() => {
                    if (!txAmount || isNaN(parseFloat(txAmount))) {
                      Alert.alert('Invalid', 'Enter a valid amount');
                      return;
                    }
                    recordTransactionMutation.mutate();
                  }}
                >
                  <Text style={styles.inlineBtnPrimaryText}>Record</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.inlineBtn, styles.inlineBtnSecondary]}
                  onPress={() => setShowTxForm(false)}
                >
                  <Text style={styles.inlineBtnSecondaryText}>Cancel</Text>
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <TouchableOpacity style={styles.recordBtn} onPress={() => setShowTxForm(true)}>
              <Text style={styles.recordBtnText}>+ Record Transaction</Text>
            </TouchableOpacity>
          )}

          {/* Transaction History */}
          {(partner.transactions ?? []).length > 0 && (
            <View style={styles.txHistory}>
              <Text style={styles.txHistoryTitle}>Recent Transactions</Text>
              {(partner.transactions ?? []).slice(0, 10).map((tx) => (
                <View key={tx.id} style={styles.txRow}>
                  <View style={styles.txInfo}>
                    <Text style={styles.txType}>{tx.type}</Text>
                    {tx.notes && <Text style={styles.txNotes} numberOfLines={1}>{tx.notes}</Text>}
                    <Text style={styles.txDate}>{new Date(tx.createdAt).toLocaleDateString('tr-TR')}</Text>
                  </View>
                  <Text style={[
                    styles.txAmount,
                    { color: tx.type === 'PaymentCredit' ? Colors.success : Colors.error },
                  ]}>
                    {tx.type === 'PaymentCredit' ? '−' : '+'}₺{tx.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                  </Text>
                </View>
              ))}
            </View>
          )}
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
  // Pricing
  pricingRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  pricingValue: { fontSize: 22, fontWeight: '800', color: Colors.primary, marginTop: 2 },
  editBtn: { paddingHorizontal: 14, paddingVertical: 6, borderRadius: 8, borderWidth: 1.5, borderColor: Colors.primary },
  editBtnText: { fontSize: 13, fontWeight: '700', color: Colors.primary },
  input: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    padding: 10,
    fontSize: 14,
    color: Colors.text,
    backgroundColor: Colors.background,
    marginBottom: 8,
  },
  inputMultiline: { minHeight: 56, textAlignVertical: 'top' },
  inlineActions: { flexDirection: 'row', gap: 8, marginTop: 4 },
  inlineBtn: { flex: 1, paddingVertical: 9, borderRadius: 8, alignItems: 'center' },
  inlineBtnPrimary: { backgroundColor: Colors.primary },
  inlineBtnPrimaryText: { fontSize: 13, fontWeight: '700', color: '#fff' },
  inlineBtnSecondary: { backgroundColor: Colors.grayLight, borderWidth: 1, borderColor: Colors.border },
  inlineBtnSecondaryText: { fontSize: 13, fontWeight: '700', color: Colors.textSecondary },
  // Current Account
  accountHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 2 },
  balanceValue: { fontSize: 20, fontWeight: '800' },
  balanceHint: { fontSize: 11, color: Colors.textMuted, marginBottom: 12 },
  recordBtn: {
    paddingVertical: 9,
    borderRadius: 8,
    borderWidth: 1.5,
    borderColor: Colors.primary,
    alignItems: 'center',
    marginBottom: 8,
  },
  recordBtnText: { fontSize: 13, fontWeight: '700', color: Colors.primary },
  txForm: { marginTop: 4 },
  txFormTitle: { fontSize: 13, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  txLabel: { fontSize: 12, color: Colors.textSecondary, marginBottom: 4 },
  typeChips: { flexDirection: 'row', flexWrap: 'wrap', gap: 6, marginBottom: 10 },
  typeChip: {
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
  },
  typeChipActive: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  typeChipText: { fontSize: 11, fontWeight: '600', color: Colors.textSecondary },
  typeChipTextActive: { color: '#fff' },
  txHistory: { marginTop: 14, borderTopWidth: 1, borderTopColor: Colors.borderLight, paddingTop: 12 },
  txHistoryTitle: { fontSize: 12, fontWeight: '700', color: Colors.textSecondary, marginBottom: 8 },
  txRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  txInfo: { flex: 1 },
  txType: { fontSize: 12, fontWeight: '700', color: Colors.text },
  txNotes: { fontSize: 11, color: Colors.textSecondary, marginTop: 1 },
  txDate: { fontSize: 10, color: Colors.textMuted, marginTop: 2 },
  txAmount: { fontSize: 13, fontWeight: '700', marginLeft: 8 },
});

import { useQuery, useQueryClient } from '@tanstack/react-query';
import * as WebBrowser from 'expo-web-browser';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Alert, FlatList, Modal, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Card } from '../../components/ui/Card';
import { ErrorState } from '../../components/ui/ErrorState';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { ordersApi } from '../../lib/api/orders';
import { partnersApi } from '../../lib/api/partners';
import { api } from '../../lib/api';

// Unified ledger entry combining account transactions and priced orders
interface LedgerEntry {
  id: string;
  label: string;
  sublabel: string | null;
  amount: number;
  date: string;
  isDebit: boolean;
}

function EntryRow({ entry }: { entry: LedgerEntry }) {
  const amountColor = entry.isDebit ? Colors.error : Colors.success;
  const tagBg = entry.isDebit ? Colors.errorLight : Colors.successLight;

  const date = new Date(entry.date).toLocaleDateString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  return (
    <View style={styles.row}>
      <View style={[styles.tag, { backgroundColor: tagBg }]}>
        <Text style={[styles.tagText, { color: amountColor }]}>{entry.label}</Text>
      </View>
      <View style={styles.rowBody}>
        {entry.sublabel ? <Text style={styles.sublabel}>{entry.sublabel}</Text> : null}
        <Text style={styles.rowDate}>{date}</Text>
      </View>
      <Text style={[styles.rowAmount, { color: amountColor }]}>
        ₺{entry.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
      </Text>
    </View>
  );
}

export default function AccountScreen() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [payModalVisible, setPayModalVisible] = useState(false);
  const [payAmount, setPayAmount] = useState('');
  const [paying, setPaying] = useState(false);

  const {
    data: account,
    isLoading: accountLoading,
    isError: accountError,
    refetch: refetchAccount,
  } = useQuery({
    queryKey: ['partner-account'],
    queryFn: () => partnersApi.getAccount().then((r) => r.data),
  });

  const {
    data: ordersData,
    isLoading: ordersLoading,
    isError: ordersError,
    refetch: refetchOrders,
  } = useQuery({
    queryKey: ['partner-orders-ledger'],
    queryFn: () => ordersApi.getOrders({ pageNumber: 1, pageSize: 200 }).then((r) => r.data),
  });

  if (accountLoading || ordersLoading) return <LoadingScreen />;
  if (accountError || ordersError || !account) {
    return <ErrorState onRetry={() => { refetchAccount(); refetchOrders(); }} />;
  }

  // Build unified sorted ledger
  const typeLabels: Record<string, { label: string; isDebit: boolean }> = {
    OrderDebit: { label: t('account.orderDebit'), isDebit: true },
    PaymentCredit: { label: t('account.paymentCredit'), isDebit: false },
    ManualAdjustment: { label: t('account.manualAdjustment'), isDebit: false },
  };

  const entries: LedgerEntry[] = [];

  // Manual account transactions (recorded by admin)
  for (const tx of account.transactions) {
    const meta = typeLabels[tx.type] ?? { label: tx.type, isDebit: true };
    const parts: string[] = [];
    if (tx.orderReference) parts.push(`${t('account.orderRef')} ${tx.orderReference}`);
    if (tx.paymentMethod) parts.push(`${t('account.paymentMethod')} ${tx.paymentMethod}`);
    if (tx.notes) parts.push(tx.notes);

    entries.push({
      id: tx.id,
      label: meta.label,
      sublabel: parts.length > 0 ? parts.join(' · ') : null,
      amount: tx.amount,
      date: tx.createdAt,
      isDebit: meta.isDebit,
    });
  }

  // Priced orders — shown as order debits if not already covered by an account transaction
  const coveredOrderRefs = new Set(
    account.transactions.filter((tx) => tx.orderReference).map((tx) => tx.orderReference!)
  );

  for (const order of ordersData?.items ?? []) {
    if (!order.totalAmount || order.totalAmount <= 0) continue;
    if (coveredOrderRefs.has(order.orderNumber)) continue;

    entries.push({
      id: `order-${order.id}`,
      label: t('account.orderEntry'),
      sublabel: order.orderNumber,
      amount: order.totalAmount,
      date: order.createdAt,
      isDebit: true,
    });
  }

  entries.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

  const handlePayment = async () => {
    const parsed = parseFloat(payAmount.replace(',', '.'));
    if (!parsed || parsed <= 0) {
      Alert.alert('Hata', 'Geçerli bir tutar girin.');
      return;
    }
    setPaying(true);
    try {
      const response = await api.post<{ token: string }>('/api/identity/partners/payments/initialize', { amount: parsed });
      const token = response.data.token;
      const baseUrl = (api.defaults.baseURL ?? 'http://localhost:8080').replace(/\/$/, '');
      const formUrl = `${baseUrl}/api/identity/partners/payments/form/${token}`;
      setPayModalVisible(false);
      setPayAmount('');
      const result = await WebBrowser.openBrowserAsync(formUrl);
      if (result.type === 'cancel' || result.type === 'dismiss') {
        // Refresh account after browser closes
        queryClient.invalidateQueries({ queryKey: ['partner-account'] });
      }
    } catch {
      Alert.alert('Hata', 'Ödeme başlatılamadı. Lütfen tekrar deneyin.');
    } finally {
      setPaying(false);
    }
  };

  const balance = account.currentBalance;
  const balanceColor =
    balance === 0 ? Colors.textSecondary : balance > 0 ? Colors.error : Colors.success;
  const balanceLabel =
    balance === 0 ? t('account.noDebt') : balance > 0 ? t('account.inDebt') : t('account.inCredit');

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        contentContainerStyle={styles.content}
        data={entries}
        keyExtractor={(item) => item.id}
        ListHeaderComponent={
          <>
            <Card style={styles.balanceCard}>
              <Text style={styles.balanceLabel}>{t('account.currentBalance')}</Text>
              <Text style={[styles.balanceAmount, { color: balanceColor }]}>
                ₺{Math.abs(balance).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
              </Text>
              <View style={[styles.balanceBadge, { backgroundColor: balanceColor + '20' }]}>
                <Text style={[styles.balanceBadgeText, { color: balanceColor }]}>{balanceLabel}</Text>
              </View>
              {account.discountRate > 0 && (
                <Text style={styles.discountText}>
                  {t('account.discountRate')}: %{account.discountRate}
                </Text>
              )}
              {balance > 0 && (
                <TouchableOpacity
                  style={styles.payButton}
                  onPress={() => { setPayAmount(balance.toFixed(2)); setPayModalVisible(true); }}
                >
                  <Text style={styles.payButtonText}>Ödeme Yap</Text>
                </TouchableOpacity>
              )}
            </Card>

            {/* Payment modal */}
            <Modal visible={payModalVisible} transparent animationType="slide">
              <View style={styles.modalOverlay}>
                <View style={styles.modalBox}>
                  <Text style={styles.modalTitle}>Ödeme Yap</Text>
                  <Text style={styles.modalLabel}>Tutar (₺)</Text>
                  <TextInput
                    style={styles.modalInput}
                    keyboardType="decimal-pad"
                    value={payAmount}
                    onChangeText={setPayAmount}
                    placeholder="0.00"
                  />
                  {balance > 0 && (
                    <TouchableOpacity onPress={() => setPayAmount(balance.toFixed(2))}>
                      <Text style={styles.fullDebtLink}>
                        Tüm borcu öde (₺{balance.toLocaleString('tr-TR', { minimumFractionDigits: 2 })})
                      </Text>
                    </TouchableOpacity>
                  )}
                  <View style={styles.modalActions}>
                    <TouchableOpacity style={styles.cancelBtn} onPress={() => setPayModalVisible(false)}>
                      <Text style={styles.cancelBtnText}>İptal</Text>
                    </TouchableOpacity>
                    <TouchableOpacity
                      style={[styles.confirmBtn, paying && styles.confirmBtnDisabled]}
                      onPress={handlePayment}
                      disabled={paying}
                    >
                      <Text style={styles.confirmBtnText}>{paying ? 'Yükleniyor...' : 'Devam Et'}</Text>
                    </TouchableOpacity>
                  </View>
                </View>
              </View>
            </Modal>
            <Text style={styles.sectionTitle}>{t('account.transactions')}</Text>
          </>
        }
        ListEmptyComponent={
          <Card style={styles.emptyCard}>
            <Text style={styles.emptyText}>{t('account.noTransactions')}</Text>
          </Card>
        }
        renderItem={({ item }) => <EntryRow entry={item} />}
        ItemSeparatorComponent={() => <View style={styles.separator} />}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },

  balanceCard: { alignItems: 'center', paddingVertical: 28, marginBottom: 24 },
  balanceLabel: { fontSize: 13, color: Colors.textSecondary, marginBottom: 8 },
  balanceAmount: { fontSize: 40, fontWeight: '800', marginBottom: 10 },
  balanceBadge: { paddingHorizontal: 14, paddingVertical: 4, borderRadius: 20, marginBottom: 12 },
  balanceBadgeText: { fontSize: 13, fontWeight: '700' },
  discountText: { fontSize: 13, color: Colors.primary, fontWeight: '600' },

  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 8 },

  row: {
    backgroundColor: Colors.surface,
    borderRadius: 10,
    padding: 14,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  tag: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
    alignSelf: 'flex-start',
    minWidth: 76,
    alignItems: 'center',
  },
  tagText: { fontSize: 11, fontWeight: '700' },
  rowBody: { flex: 1, gap: 2 },
  sublabel: { fontSize: 12, color: Colors.textSecondary },
  rowDate: { fontSize: 11, color: Colors.textMuted },
  rowAmount: { fontSize: 15, fontWeight: '700' },

  separator: { height: 8 },
  emptyCard: { alignItems: 'center', paddingVertical: 32 },
  emptyText: { fontSize: 14, color: Colors.textMuted },

  payButton: {
    marginTop: 16,
    backgroundColor: Colors.primary,
    paddingHorizontal: 28,
    paddingVertical: 10,
    borderRadius: 20,
  },
  payButtonText: { color: '#fff', fontWeight: '700', fontSize: 14 },

  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.45)',
    justifyContent: 'flex-end',
  },
  modalBox: {
    backgroundColor: Colors.surface,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 24,
    paddingBottom: 36,
  },
  modalTitle: { fontSize: 18, fontWeight: '800', color: Colors.text, marginBottom: 20 },
  modalLabel: { fontSize: 13, color: Colors.textSecondary, marginBottom: 6 },
  modalInput: {
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: 10,
    padding: 12,
    fontSize: 18,
    color: Colors.text,
    marginBottom: 8,
  },
  fullDebtLink: { fontSize: 13, color: Colors.primary, marginBottom: 20 },
  modalActions: { flexDirection: 'row', gap: 10, marginTop: 8 },
  cancelBtn: {
    flex: 1,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: 10,
    paddingVertical: 12,
    alignItems: 'center',
  },
  cancelBtnText: { color: Colors.text, fontWeight: '600' },
  confirmBtn: {
    flex: 2,
    backgroundColor: Colors.primary,
    borderRadius: 10,
    paddingVertical: 12,
    alignItems: 'center',
  },
  confirmBtnDisabled: { opacity: 0.5 },
  confirmBtnText: { color: '#fff', fontWeight: '700' },
});

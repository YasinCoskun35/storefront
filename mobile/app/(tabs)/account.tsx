import { useQuery } from '@tanstack/react-query';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Card } from '../../components/ui/Card';
import { ErrorState } from '../../components/ui/ErrorState';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { ordersApi } from '../../lib/api/orders';
import { partnersApi } from '../../lib/api/partners';

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
            </Card>
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
});

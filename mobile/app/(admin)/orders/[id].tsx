import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocalSearchParams, useRouter } from 'expo-router';
import React, { useState } from 'react';
import {
  Alert,
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Button } from '../../../components/ui/Button';
import { Card } from '../../../components/ui/Card';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { adminApi } from '../../../lib/api/admin';

const STATUS_TRANSITIONS: Record<string, string[]> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['InProduction', 'Cancelled'],
  InProduction: ['ReadyToShip', 'Cancelled'],
  ReadyToShip: ['Shipping'],
  Shipping: ['Delivered'],
  Delivered: ['Completed'],
  QuoteSent: ['Confirmed', 'Cancelled'],
};

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

export default function AdminOrderDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();

  const [statusModal, setStatusModal] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState('');
  const [statusNotes, setStatusNotes] = useState('');
  const [commentText, setCommentText] = useState('');

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-order', id],
    queryFn: () => adminApi.getOrderDetails(id),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ status, notes }: { status: string; notes: string }) =>
      adminApi.updateOrderStatus(id, status, notes || undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-order', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-order-stats'] });
      setStatusModal(false);
      setStatusNotes('');
    },
    onError: (err: any) => {
      Alert.alert('Error', err.response?.data?.message ?? 'Failed to update status');
    },
  });

  const addCommentMutation = useMutation({
    mutationFn: () => adminApi.addComment(id, commentText.trim(), false),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-order', id] });
      setCommentText('');
    },
    onError: (err: any) => {
      Alert.alert('Error', err.response?.data?.message ?? 'Failed to add comment');
    },
  });

  if (isLoading) return <LoadingScreen />;
  if (isError || !data?.data) return <ErrorState onRetry={refetch} />;

  const order = data.data;
  const nextStatuses = STATUS_TRANSITIONS[order.status] ?? [];

  function openStatusModal(status: string) {
    setSelectedStatus(status);
    setStatusModal(true);
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>

        {/* Header */}
        <Card style={styles.section}>
          <View style={styles.orderHeader}>
            <Text style={styles.orderNumber}>{order.orderNumber}</Text>
            <View style={[styles.badge, { backgroundColor: (STATUS_COLORS[order.status] ?? Colors.gray) + '20' }]}>
              <Text style={[styles.badgeText, { color: STATUS_COLORS[order.status] ?? Colors.gray }]}>
                {order.status}
              </Text>
            </View>
          </View>
          <Text style={styles.company}>{order.partnerCompanyName}</Text>
          <Text style={styles.date}>{new Date(order.createdAt).toLocaleString()}</Text>
        </Card>

        {/* Status actions */}
        {nextStatuses.length > 0 && (
          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Update Status</Text>
            <View style={styles.statusButtons}>
              {nextStatuses.map((s) => (
                <TouchableOpacity
                  key={s}
                  style={[styles.statusBtn, { borderColor: STATUS_COLORS[s] ?? Colors.border }]}
                  onPress={() => openStatusModal(s)}
                >
                  <Text style={[styles.statusBtnText, { color: STATUS_COLORS[s] ?? Colors.text }]}>
                    → {s}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </Card>
        )}

        {/* Delivery address */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Delivery Address</Text>
          <Text style={styles.addressText}>
            {order.deliveryAddress}{'\n'}
            {order.deliveryCity}, {order.deliveryState} {order.deliveryPostalCode}{'\n'}
            {order.deliveryCountry}
          </Text>
          {order.deliveryNotes && (
            <Text style={styles.notes}>Note: {order.deliveryNotes}</Text>
          )}
        </Card>

        {/* Items */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Items ({order.items?.length ?? 0})</Text>
          {order.items?.map((item, i) => (
            <View key={item.id} style={[styles.itemRow, i < (order.items?.length ?? 0) - 1 && styles.itemBorder]}>
              <View style={styles.itemInfo}>
                <Text style={styles.itemName}>{item.productName}</Text>
                <Text style={styles.itemSku}>{item.productSKU}</Text>
                {item.selectedVariants ? (() => {
                  try {
                    return (JSON.parse(item.selectedVariants) as Array<{groupName: string; optionName: string; optionCode: string}>)
                      .map((v, i) => (
                        <Text key={i} style={styles.itemColor}>{v.groupName}: {v.optionName}{v.optionCode ? ` (${v.optionCode})` : ''}</Text>
                      ));
                  } catch { return null; }
                })() : null}
              </View>
              <Text style={styles.itemQty}>×{item.quantity}</Text>
            </View>
          ))}
        </Card>

        {/* Comments */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Comments</Text>
          {(order.comments ?? []).length === 0 ? (
            <Text style={styles.emptyText}>No comments yet.</Text>
          ) : (
            order.comments?.map((c) => (
              <View key={c.id} style={styles.comment}>
                <View style={styles.commentHeader}>
                  <Text style={styles.commentAuthor}>{c.authorName}</Text>
                  <Text style={styles.commentDate}>{new Date(c.createdAt).toLocaleDateString()}</Text>
                </View>
                <Text style={styles.commentText}>{c.content}</Text>
              </View>
            ))
          )}

          <View style={styles.addComment}>
            <TextInput
              style={styles.commentInput}
              value={commentText}
              onChangeText={setCommentText}
              placeholder="Add a comment..."
              placeholderTextColor={Colors.textMuted}
              multiline
            />
            <Button
              title="Send"
              onPress={() => {
                if (!commentText.trim()) return;
                addCommentMutation.mutate();
              }}
              loading={addCommentMutation.isPending}
              size="sm"
              style={styles.sendBtn}
            />
          </View>
        </Card>
      </ScrollView>

      {/* Status update modal */}
      <Modal visible={statusModal} transparent animationType="slide">
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Update to "{selectedStatus}"</Text>
            <TextInput
              style={styles.modalInput}
              value={statusNotes}
              onChangeText={setStatusNotes}
              placeholder="Optional notes..."
              placeholderTextColor={Colors.textMuted}
              multiline
            />
            <View style={styles.modalButtons}>
              <Button
                title="Cancel"
                variant="outline"
                onPress={() => setStatusModal(false)}
                style={styles.modalBtn}
              />
              <Button
                title="Confirm"
                onPress={() => updateStatusMutation.mutate({ status: selectedStatus, notes: statusNotes })}
                loading={updateStatusMutation.isPending}
                style={styles.modalBtn}
              />
            </View>
          </View>
        </View>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },
  section: { marginBottom: 16 },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  orderHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 },
  orderNumber: { fontSize: 18, fontWeight: '800', color: Colors.text },
  badge: { borderRadius: 6, paddingHorizontal: 10, paddingVertical: 3 },
  badgeText: { fontSize: 12, fontWeight: '700' },
  company: { fontSize: 14, color: Colors.textSecondary, marginBottom: 2 },
  date: { fontSize: 12, color: Colors.textMuted },
  statusButtons: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  statusBtn: {
    borderWidth: 1.5,
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 8,
  },
  statusBtnText: { fontSize: 13, fontWeight: '700' },
  addressText: { fontSize: 13, color: Colors.text, lineHeight: 20 },
  notes: { fontSize: 12, color: Colors.textSecondary, marginTop: 6, fontStyle: 'italic' },
  itemRow: { flexDirection: 'row', alignItems: 'center', paddingVertical: 10 },
  itemBorder: { borderBottomWidth: 1, borderBottomColor: Colors.borderLight },
  itemInfo: { flex: 1 },
  itemName: { fontSize: 13, fontWeight: '600', color: Colors.text },
  itemSku: { fontSize: 11, color: Colors.textMuted, marginTop: 1 },
  itemColor: { fontSize: 11, color: Colors.textSecondary, marginTop: 2 },
  itemQty: { fontSize: 14, fontWeight: '700', color: Colors.text },
  comment: { marginBottom: 12 },
  commentHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 3 },
  commentAuthor: { fontSize: 12, fontWeight: '700', color: Colors.text },
  commentDate: { fontSize: 11, color: Colors.textMuted },
  commentText: { fontSize: 13, color: Colors.text },
  emptyText: { fontSize: 13, color: Colors.textMuted, textAlign: 'center', paddingVertical: 8 },
  addComment: { marginTop: 12, gap: 8 },
  commentInput: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    padding: 10,
    fontSize: 13,
    color: Colors.text,
    minHeight: 64,
    textAlignVertical: 'top',
  },
  sendBtn: { alignSelf: 'flex-end' },
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContent: {
    backgroundColor: Colors.surface,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 24,
    paddingBottom: 40,
  },
  modalTitle: { fontSize: 18, fontWeight: '700', color: Colors.text, marginBottom: 16 },
  modalInput: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    padding: 12,
    fontSize: 14,
    color: Colors.text,
    minHeight: 80,
    textAlignVertical: 'top',
    marginBottom: 16,
  },
  modalButtons: { flexDirection: 'row', gap: 10 },
  modalBtn: { flex: 1 },
});

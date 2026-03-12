import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocalSearchParams } from 'expo-router';
import React, { useState } from 'react';
import {
  Alert,
  Image,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { StatusBadge } from '../../../components/orders/StatusBadge';
import { Button } from '../../../components/ui/Button';
import { Card } from '../../../components/ui/Card';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { ordersApi } from '../../../lib/api/orders';
import { getImageUrl } from '../../../lib/api';
import type { OrderComment } from '../../../lib/types';

const STATUS_TIMELINE = [
  'Pending',
  'QuoteSent',
  'Confirmed',
  'InProduction',
  'ReadyToShip',
  'Shipping',
  'Delivered',
  'Completed',
];

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatDateShort(dateString: string) {
  return new Date(dateString).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

export default function OrderDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [comment, setComment] = useState('');
  const [cancelReason, setCancelReason] = useState('');

  const { data: order, isLoading, isError, refetch } = useQuery({
    queryKey: ['order', id],
    queryFn: () => ordersApi.getOrderDetails(id).then((r) => r.data),
    enabled: !!id,
  });

  const commentMutation = useMutation({
    mutationFn: (content: string) => ordersApi.addComment(id, content),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['order', id] });
      setComment('');
    },
    onError: () => Alert.alert('Error', 'Failed to add comment.'),
  });

  const cancelMutation = useMutation({
    mutationFn: (reason: string) => ordersApi.cancelOrder(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['order', id] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['order-stats'] });
      setCancelReason('');
    },
    onError: (err: any) => {
      Alert.alert('Error', err.response?.data?.message ?? 'Failed to cancel order.');
    },
  });

  function handleCancelOrder() {
    Alert.prompt(
      'Cancel Order',
      'Please provide a reason for cancellation:',
      [
        { text: 'Keep Order', style: 'cancel' },
        {
          text: 'Cancel Order',
          style: 'destructive',
          onPress: (reason: string | undefined) => {
            if (!reason?.trim()) {
              Alert.alert('Required', 'A cancellation reason is required.');
              return;
            }
            cancelMutation.mutate(reason.trim());
          },
        },
      ],
      'plain-text',
      '',
    );
  }

  if (isLoading) return <LoadingScreen />;
  if (isError || !order) return <ErrorState onRetry={refetch} />;

  const currentStatusIndex = STATUS_TIMELINE.indexOf(order.status);
  const canCancel = order.status === 'Pending' || order.status === 'QuoteSent';

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>
        {/* Header */}
        <Card style={styles.section}>
          <View style={styles.headerRow}>
            <Text style={styles.orderNumber}>{order.orderNumber}</Text>
            <StatusBadge status={order.status} />
          </View>
          <Text style={styles.meta}>Placed {formatDateShort(order.createdAt)}</Text>
        </Card>

        {/* Timeline */}
        {order.status !== 'Cancelled' && (
          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Order Progress</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false}>
              <View style={styles.timeline}>
                {STATUS_TIMELINE.map((status, idx) => {
                  const done = idx < currentStatusIndex;
                  const active = idx === currentStatusIndex;
                  return (
                    <View key={status} style={styles.timelineStep}>
                      <View style={[
                        styles.timelineDot,
                        done && styles.timelineDotDone,
                        active && styles.timelineDotActive,
                      ]} />
                      {idx < STATUS_TIMELINE.length - 1 && (
                        <View style={[styles.timelineLine, done && styles.timelineLineDone]} />
                      )}
                      <Text style={[
                        styles.timelineLabel,
                        active && styles.timelineLabelActive,
                        done && styles.timelineLabelDone,
                      ]} numberOfLines={2}>
                        {status === 'QuoteSent' ? 'Quote' :
                         status === 'InProduction' ? 'Production' :
                         status === 'ReadyToShip' ? 'Ready' : status}
                      </Text>
                    </View>
                  );
                })}
              </View>
            </ScrollView>
          </Card>
        )}

        {/* Items */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Items ({order.items.length})</Text>
          {order.items.map((item) => (
            <View key={item.id} style={styles.itemRow}>
              {item.productImageUrl ? (
                <Image
                  source={{ uri: getImageUrl(item.productImageUrl) }}
                  style={styles.itemImage}
                  resizeMode="cover"
                />
              ) : (
                <View style={[styles.itemImage, styles.imagePlaceholder]} />
              )}
              <View style={styles.itemInfo}>
                <Text style={styles.itemName} numberOfLines={2}>{item.productName}</Text>
                <Text style={styles.itemSku}>SKU: {item.productSKU}</Text>
                <Text style={styles.itemQty}>Qty: {item.quantity}</Text>
                {item.selectedVariants ? (() => {
                  try {
                    return (JSON.parse(item.selectedVariants) as Array<{groupName: string; optionName: string; optionCode: string}>)
                      .map((v, i) => (
                        <Text key={i} style={styles.itemColor}>
                          {v.groupName}: {v.optionName}{v.optionCode ? ` (${v.optionCode})` : ''}
                        </Text>
                      ));
                  } catch { return null; }
                })() : null}
                {item.customizationNotes ? (
                  <Text style={styles.itemNotes}>{item.customizationNotes}</Text>
                ) : null}
              </View>
            </View>
          ))}
        </Card>

        {/* Delivery Address */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Delivery Address</Text>
          <Text style={styles.bodyText}>
            {order.deliveryAddress}{'\n'}
            {order.deliveryCity}, {order.deliveryState} {order.deliveryPostalCode}{'\n'}
            {order.deliveryCountry}
          </Text>
          {order.deliveryNotes ? (
            <Text style={styles.meta}>Notes: {order.deliveryNotes}</Text>
          ) : null}
          {order.requestedDeliveryDate ? (
            <Text style={styles.meta}>Requested: {formatDateShort(order.requestedDeliveryDate)}</Text>
          ) : null}
        </Card>

        {/* Shipping Info */}
        {order.shippingInfo?.trackingNumber ? (
          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Shipping Info</Text>
            {order.shippingInfo.carrier ? (
              <Text style={styles.bodyText}>Carrier: {order.shippingInfo.carrier}</Text>
            ) : null}
            <Text style={styles.bodyText}>
              Tracking: {order.shippingInfo.trackingNumber}
            </Text>
            {order.shippingInfo.shippedAt ? (
              <Text style={styles.meta}>Shipped: {formatDateShort(order.shippingInfo.shippedAt)}</Text>
            ) : null}
            {order.shippingInfo.estimatedDelivery ? (
              <Text style={styles.meta}>
                Est. Delivery: {formatDateShort(order.shippingInfo.estimatedDelivery)}
              </Text>
            ) : null}
          </Card>
        ) : null}

        {/* Comments */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Comments</Text>
          {order.comments.length === 0 ? (
            <Text style={styles.emptyText}>No comments yet.</Text>
          ) : (
            order.comments.map((c: OrderComment) => (
              <View key={c.id} style={styles.commentItem}>
                <View style={styles.commentHeader}>
                  <Text style={styles.commentAuthor}>{c.authorName}</Text>
                  <Text style={styles.commentDate}>{formatDate(c.createdAt)}</Text>
                </View>
                <Text style={styles.commentContent}>{c.content}</Text>
              </View>
            ))
          )}
          <View style={styles.addComment}>
            <TextInput
              style={styles.commentInput}
              value={comment}
              onChangeText={setComment}
              placeholder="Add a comment..."
              placeholderTextColor={Colors.textMuted}
              multiline
              numberOfLines={2}
              textAlignVertical="top"
            />
            <Button
              title="Send"
              variant="outline"
              size="sm"
              disabled={!comment.trim()}
              loading={commentMutation.isPending}
              onPress={() => comment.trim() && commentMutation.mutate(comment.trim())}
              style={styles.sendBtn}
            />
          </View>
        </Card>

        {/* Cancel Order */}
        {canCancel && (
          <Button
            title="Cancel Order"
            variant="danger"
            onPress={handleCancelOrder}
            loading={cancelMutation.isPending}
            style={styles.cancelButton}
          />
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },
  section: { marginBottom: 14 },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 6,
  },
  orderNumber: { fontSize: 18, fontWeight: '800', color: Colors.text },
  meta: { fontSize: 12, color: Colors.textSecondary, marginTop: 3 },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  bodyText: { fontSize: 14, color: Colors.text, lineHeight: 22 },
  timeline: { flexDirection: 'row', alignItems: 'flex-start', paddingBottom: 8 },
  timelineStep: { alignItems: 'center', width: 56, position: 'relative' },
  timelineDot: {
    width: 14,
    height: 14,
    borderRadius: 7,
    backgroundColor: Colors.border,
    borderWidth: 2,
    borderColor: Colors.border,
    marginBottom: 6,
    zIndex: 1,
  },
  timelineDotDone: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  timelineDotActive: { backgroundColor: Colors.primary, borderColor: Colors.primary, width: 18, height: 18, borderRadius: 9 },
  timelineLine: {
    position: 'absolute',
    top: 6,
    left: '50%',
    width: 56,
    height: 2,
    backgroundColor: Colors.border,
    zIndex: 0,
  },
  timelineLineDone: { backgroundColor: Colors.primary },
  timelineLabel: { fontSize: 9, color: Colors.textMuted, textAlign: 'center', fontWeight: '500' },
  timelineLabelActive: { color: Colors.primary, fontWeight: '700' },
  timelineLabelDone: { color: Colors.textSecondary },
  itemRow: {
    flexDirection: 'row',
    gap: 10,
    paddingVertical: 8,
    borderTopWidth: 1,
    borderTopColor: Colors.borderLight,
  },
  itemImage: {
    width: 60,
    height: 60,
    borderRadius: 8,
    backgroundColor: Colors.borderLight,
  },
  imagePlaceholder: {},
  itemInfo: { flex: 1 },
  itemName: { fontSize: 13, fontWeight: '700', color: Colors.text, marginBottom: 2 },
  itemSku: { fontSize: 11, color: Colors.textMuted },
  itemQty: { fontSize: 12, color: Colors.textSecondary },
  itemColor: { fontSize: 11, color: Colors.primary },
  itemNotes: { fontSize: 11, color: Colors.textSecondary, fontStyle: 'italic' },
  commentItem: {
    paddingVertical: 10,
    borderTopWidth: 1,
    borderTopColor: Colors.borderLight,
  },
  commentHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 },
  commentAuthor: { fontSize: 12, fontWeight: '700', color: Colors.text },
  commentDate: { fontSize: 11, color: Colors.textMuted },
  commentContent: { fontSize: 13, color: Colors.text, lineHeight: 20 },
  addComment: { marginTop: 12, gap: 8 },
  commentInput: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    padding: 10,
    fontSize: 13,
    color: Colors.text,
    minHeight: 60,
    backgroundColor: Colors.background,
  },
  sendBtn: { alignSelf: 'flex-end' },
  emptyText: { fontSize: 13, color: Colors.textMuted, paddingVertical: 8 },
  cancelButton: { marginTop: 4 },
});

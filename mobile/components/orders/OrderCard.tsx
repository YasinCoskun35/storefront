import React from 'react';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '../../constants/colors';
import type { OrderSummary } from '../../lib/types';
import { StatusBadge } from './StatusBadge';

interface OrderCardProps {
  order: OrderSummary;
  onPress: () => void;
}

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function OrderCard({ order, onPress }: OrderCardProps) {
  return (
    <TouchableOpacity style={styles.card} onPress={onPress} activeOpacity={0.8}>
      <View style={styles.row}>
        <Text style={styles.orderNumber}>{order.orderNumber}</Text>
        <StatusBadge status={order.status} />
      </View>
      <View style={styles.row}>
        <Text style={styles.meta}>{order.itemCount} item{order.itemCount !== 1 ? 's' : ''}</Text>
        <Text style={styles.meta}>{formatDate(order.createdAt)}</Text>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: Colors.surface,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    padding: 14,
    marginBottom: 8,
    gap: 8,
  },
  row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  orderNumber: { fontSize: 14, fontWeight: '700', color: Colors.text },
  meta: { fontSize: 12, color: Colors.textSecondary },
});

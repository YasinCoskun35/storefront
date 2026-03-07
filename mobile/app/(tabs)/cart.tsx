import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React from 'react';
import {
  Alert,
  FlatList,
  Image,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Button } from '../../components/ui/Button';
import { EmptyState } from '../../components/ui/EmptyState';
import { ErrorState } from '../../components/ui/ErrorState';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { ordersApi } from '../../lib/api/orders';
import { getImageUrl } from '../../lib/api';
import type { CartItem } from '../../lib/types';

function CartItemRow({ item, onUpdateQty, onRemove }: {
  item: CartItem;
  onUpdateQty: (newQty: number) => void;
  onRemove: () => void;
}) {
  return (
    <View style={styles.itemCard}>
      {item.productImageUrl ? (
        <Image
          source={{ uri: getImageUrl(item.productImageUrl) }}
          style={styles.itemImage}
          resizeMode="cover"
        />
      ) : (
        <View style={[styles.itemImage, styles.imagePlaceholder]}>
          <Text style={styles.imagePlaceholderText}>No Image</Text>
        </View>
      )}

      <View style={styles.itemInfo}>
        <Text style={styles.itemName} numberOfLines={2}>{item.productName}</Text>
        <Text style={styles.itemSku}>SKU: {item.productSKU}</Text>
        {item.selectedVariants ? (() => {
          try {
            const variants = JSON.parse(item.selectedVariants) as Array<{
              groupName: string; optionName: string; optionCode: string;
            }>;
            return variants.map((v, i) => (
              <Text key={i} style={styles.itemColor}>
                {v.groupName}: {v.optionName}{v.optionCode ? ` (${v.optionCode})` : ''}
              </Text>
            ));
          } catch { return null; }
        })() : null}
        {item.customizationNotes ? (
          <Text style={styles.itemNotes} numberOfLines={2}>{item.customizationNotes}</Text>
        ) : null}

        <View style={styles.itemActions}>
          <View style={styles.qtyRow}>
            <TouchableOpacity
              style={styles.qtyBtn}
              onPress={() => onUpdateQty(Math.max(1, item.quantity - 1))}
            >
              <Text style={styles.qtyBtnText}>−</Text>
            </TouchableOpacity>
            <Text style={styles.qtyValue}>{item.quantity}</Text>
            <TouchableOpacity
              style={styles.qtyBtn}
              onPress={() => onUpdateQty(item.quantity + 1)}
            >
              <Text style={styles.qtyBtnText}>+</Text>
            </TouchableOpacity>
          </View>
          <TouchableOpacity style={styles.removeBtn} onPress={onRemove}>
            <Text style={styles.removeBtnText}>Remove</Text>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
}

export default function CartScreen() {
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: cart, isLoading, isError, refetch } = useQuery({
    queryKey: ['cart'],
    queryFn: () => ordersApi.getCart().then((r) => r.data),
  });

  const updateQtyMutation = useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      ordersApi.updateCartItemQuantity(itemId, quantity),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
    onError: () => Alert.alert('Error', 'Failed to update quantity.'),
  });

  const removeMutation = useMutation({
    mutationFn: (itemId: string) => ordersApi.removeCartItem(itemId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
    onError: () => Alert.alert('Error', 'Failed to remove item.'),
  });

  function handleRemove(item: CartItem) {
    Alert.alert('Remove Item', `Remove "${item.productName}" from cart?`, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Remove',
        style: 'destructive',
        onPress: () => removeMutation.mutate(item.id),
      },
    ]);
  }

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={refetch} />;

  const items = cart?.items ?? [];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {items.length === 0 ? (
        <EmptyState
          title="Your cart is empty"
          subtitle="Browse the catalog to add products."
        />
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={false} onRefresh={refetch} />}
          renderItem={({ item }) => (
            <CartItemRow
              item={item}
              onUpdateQty={(qty) => updateQtyMutation.mutate({ itemId: item.id, quantity: qty })}
              onRemove={() => handleRemove(item)}
            />
          )}
          ListFooterComponent={
            <View style={styles.footer}>
              <Text style={styles.summary}>
                {items.length} item{items.length !== 1 ? 's' : ''} in cart
              </Text>
              <Button
                title="Proceed to Checkout"
                onPress={() => router.push('/checkout')}
                size="lg"
              />
            </View>
          }
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16, paddingBottom: 32 },
  itemCard: {
    flexDirection: 'row',
    backgroundColor: Colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: Colors.border,
    padding: 12,
    marginBottom: 10,
    gap: 12,
  },
  itemImage: {
    width: 80,
    height: 80,
    borderRadius: 8,
    backgroundColor: Colors.borderLight,
  },
  imagePlaceholder: { alignItems: 'center', justifyContent: 'center' },
  imagePlaceholderText: { fontSize: 9, color: Colors.textMuted },
  itemInfo: { flex: 1 },
  itemName: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 2 },
  itemSku: { fontSize: 11, color: Colors.textMuted, marginBottom: 2 },
  itemColor: { fontSize: 12, color: Colors.primary, marginBottom: 2 },
  itemNotes: { fontSize: 11, color: Colors.textSecondary, fontStyle: 'italic', marginBottom: 6 },
  itemActions: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 8,
  },
  qtyRow: { flexDirection: 'row', alignItems: 'center', gap: 10 },
  qtyBtn: {
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: Colors.primaryLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtyBtnText: { fontSize: 18, color: Colors.primary, fontWeight: '700', lineHeight: 22 },
  qtyValue: { fontSize: 15, fontWeight: '700', color: Colors.text, minWidth: 24, textAlign: 'center' },
  removeBtn: {
    paddingVertical: 4,
    paddingHorizontal: 10,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: Colors.errorLight,
    backgroundColor: Colors.errorLight,
  },
  removeBtnText: { fontSize: 12, color: Colors.error, fontWeight: '600' },
  footer: {
    marginTop: 16,
    gap: 12,
  },
  summary: { fontSize: 14, color: Colors.textSecondary, textAlign: 'center' },
});

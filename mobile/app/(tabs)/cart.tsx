import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React from 'react';
import { useTranslation } from 'react-i18next';
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
import { partnersApi } from '../../lib/api/partners';
import { getImageUrl } from '../../lib/api';
import type { CartItem } from '../../lib/types';

function CartItemRow({ item, discountRate, onUpdateQty, onRemove }: {
  item: CartItem;
  discountRate: number;
  onUpdateQty: (newQty: number) => void;
  onRemove: () => void;
}) {
  const { t } = useTranslation();
  const unitPrice = item.unitPrice ?? null;
  const discountedPrice = unitPrice != null ? unitPrice * (1 - discountRate / 100) : null;
  const lineTotal = discountedPrice != null ? discountedPrice * item.quantity : null;
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

        {unitPrice != null && (
          <View style={styles.itemPriceRow}>
            {discountRate > 0 && (
              <Text style={styles.itemPriceOriginal}>
                ₺{unitPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </Text>
            )}
            {discountedPrice != null && (
              <Text style={styles.itemPriceDiscounted}>
                ₺{discountedPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                {item.quantity > 1 ? ` × ${item.quantity}` : ''}
              </Text>
            )}
            {lineTotal != null && item.quantity > 1 && (
              <Text style={styles.itemLineTotal}>
                = ₺{lineTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </Text>
            )}
          </View>
        )}

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
            <Text style={styles.removeBtnText}>{t('cart.removeItem')}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
}

export default function CartScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: profileData } = useQuery({
    queryKey: ['partner-profile'],
    queryFn: () => partnersApi.getProfile().then((r) => r.data),
    staleTime: 60_000,
  });
  const discountRate = profileData?.discountRate ?? 0;

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

  const subTotal = items.reduce((acc, item) => {
    if (item.unitPrice == null) return acc;
    return acc + item.unitPrice * item.quantity;
  }, 0);
  const totalDiscount = subTotal > 0 && discountRate > 0 ? subTotal * (discountRate / 100) : 0;
  const total = subTotal - totalDiscount;
  const hasPricing = items.some((i) => i.unitPrice != null);

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {items.length === 0 ? (
        <EmptyState
          title={t('cart.empty')}
          subtitle={t('cart.emptyDesc')}
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
              discountRate={discountRate}
              onUpdateQty={(qty) => updateQtyMutation.mutate({ itemId: item.id, quantity: qty })}
              onRemove={() => handleRemove(item)}
            />
          )}
          ListFooterComponent={
            <View style={styles.footer}>
              {hasPricing && (
                <View style={styles.priceSummary}>
                  {discountRate > 0 && (
                    <View style={styles.discountBanner}>
                      <Text style={styles.discountBannerText}>{t('cart.partnerDiscount')}: −{discountRate}% {t('cart.discountApplied')}</Text>
                    </View>
                  )}
                  <View style={styles.summaryRow}>
                    <Text style={styles.summaryLabel}>{t('cart.subtotal')}</Text>
                    <Text style={styles.summaryValue}>₺{subTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</Text>
                  </View>
                  {totalDiscount > 0 && (
                    <View style={styles.summaryRow}>
                      <Text style={[styles.summaryLabel, { color: Colors.success }]}>{t('cart.partnerDiscount')}</Text>
                      <Text style={[styles.summaryValue, { color: Colors.success }]}>
                        −₺{totalDiscount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                      </Text>
                    </View>
                  )}
                  <View style={[styles.summaryRow, styles.summaryTotal]}>
                    <Text style={styles.summaryTotalLabel}>{t('cart.total')}</Text>
                    <Text style={styles.summaryTotalValue}>₺{total.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</Text>
                  </View>
                </View>
              )}
              <Text style={styles.summary}>
                {items.length} {t('common.items')}
              </Text>
              <Button
                title={t('cart.proceedToCheckout')}
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
  itemPriceRow: { flexDirection: 'row', alignItems: 'center', flexWrap: 'wrap', gap: 6, marginTop: 4 },
  itemPriceOriginal: { fontSize: 11, color: Colors.textMuted, textDecorationLine: 'line-through' },
  itemPriceDiscounted: { fontSize: 13, fontWeight: '700', color: Colors.primary },
  itemLineTotal: { fontSize: 12, color: Colors.textSecondary },
  priceSummary: {
    backgroundColor: Colors.surface,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    padding: 12,
    gap: 6,
  },
  discountBanner: {
    backgroundColor: Colors.successLight,
    borderRadius: 6,
    padding: 8,
    marginBottom: 4,
  },
  discountBannerText: { fontSize: 12, fontWeight: '600', color: Colors.success, textAlign: 'center' },
  summaryRow: { flexDirection: 'row', justifyContent: 'space-between' },
  summaryLabel: { fontSize: 13, color: Colors.textSecondary },
  summaryValue: { fontSize: 13, fontWeight: '600', color: Colors.text },
  summaryTotal: { borderTopWidth: 1, borderTopColor: Colors.border, paddingTop: 6, marginTop: 2 },
  summaryTotalLabel: { fontSize: 15, fontWeight: '700', color: Colors.text },
  summaryTotalValue: { fontSize: 15, fontWeight: '800', color: Colors.primary },
});

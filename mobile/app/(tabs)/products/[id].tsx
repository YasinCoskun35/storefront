import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocalSearchParams, useRouter } from 'expo-router';
import React, { useState } from 'react';
import {
  Alert,
  Image,
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
import { catalogApi } from '../../../lib/api/catalog';
import { ordersApi } from '../../../lib/api/orders';
import { getImageUrl } from '../../../lib/api';
import type { ProductVariantGroup, SelectedVariantItem, VariantOption } from '../../../lib/types';

export default function ProductDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();

  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState('');
  const [selectedVariants, setSelectedVariants] = useState<Record<string, SelectedVariantItem>>({});

  const { data: product, isLoading: productLoading, isError: productError, refetch } = useQuery({
    queryKey: ['product', id],
    queryFn: () => catalogApi.getProduct(id).then((r) => r.data),
    enabled: !!id,
  });

  const { data: variantGroups, isLoading: variantsLoading } = useQuery({
    queryKey: ['product-variant-groups', id],
    queryFn: () => catalogApi.getProductVariantGroups(id).then((r) => r.data),
    enabled: !!id,
  });

  const addToCartMutation = useMutation({
    mutationFn: ordersApi.addToCart,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      Alert.alert('Added to Cart', `${product?.name} has been added to your cart.`, [
        { text: 'Continue Shopping', style: 'cancel' },
        { text: 'View Cart', onPress: () => router.push('/(tabs)/cart') },
      ]);
    },
    onError: (err: any) => {
      Alert.alert('Error', err.response?.data?.message ?? 'Failed to add to cart.');
    },
  });

  if (productLoading || variantsLoading) return <LoadingScreen />;
  if (productError || !product) return <ErrorState onRetry={refetch} />;

  const requiredGroups = variantGroups?.filter((pvg) => pvg.isRequired) ?? [];
  const canAddToCart = requiredGroups.every((pvg) => selectedVariants[pvg.variantGroupId] != null);

  function handleAddToCart() {
    if (!canAddToCart) {
      Alert.alert('Selection Required', 'Please select an option for all required variant groups.');
      return;
    }

    const selectedList = Object.values(selectedVariants);
    addToCartMutation.mutate({
      productId: product!.id,
      productName: product!.name,
      productSKU: product!.sku,
      productImageUrl: product!.primaryImageUrl,
      quantity,
      selectedVariants: selectedList.length > 0 ? JSON.stringify(selectedList) : undefined,
      customizationNotes: notes.trim() || undefined,
    });
  }

  function selectOption(pvg: ProductVariantGroup, option: VariantOption) {
    const item: SelectedVariantItem = {
      groupId: pvg.variantGroup.id,
      groupName: pvg.variantGroup.name,
      optionId: option.id,
      optionName: option.name,
      optionCode: option.code,
      hexColor: option.hexColor,
    };
    setSelectedVariants((prev) => {
      const existing = prev[pvg.variantGroupId];
      if (existing?.optionId === option.id) {
        const next = { ...prev };
        delete next[pvg.variantGroupId];
        return next;
      }
      return { ...prev, [pvg.variantGroupId]: item };
    });
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>
        {/* Image */}
        <View style={styles.imageContainer}>
          {product.primaryImageUrl ? (
            <Image
              source={{ uri: getImageUrl(product.primaryImageUrl) }}
              style={styles.image}
              resizeMode="cover"
            />
          ) : (
            <View style={styles.imagePlaceholder}>
              <Text style={styles.imagePlaceholderText}>No Image</Text>
            </View>
          )}
        </View>

        <View style={styles.body}>
          {/* Header */}
          <Text style={styles.name}>{product.name}</Text>
          <Text style={styles.sku}>SKU: {product.sku}</Text>
          <Text style={styles.category}>{product.categoryName}</Text>

          {/* Description */}
          {product.description ? (
            <Card style={styles.section}>
              <Text style={styles.sectionTitle}>Description</Text>
              <Text style={styles.bodyText}>{product.description}</Text>
            </Card>
          ) : null}

          {/* Specifications */}
          {product.specifications ? (
            <Card style={styles.section}>
              <Text style={styles.sectionTitle}>Specifications</Text>
              <Text style={styles.bodyText}>{product.specifications}</Text>
            </Card>
          ) : null}

          {/* Variant Groups */}
          {variantGroups && variantGroups.length > 0 && (
            <View style={styles.section}>
              {variantGroups.map((pvg) => {
                const availableOptions = pvg.variantGroup.options.filter((o) => o.isAvailable);
                const currentSelection = selectedVariants[pvg.variantGroupId];

                return (
                  <Card key={pvg.id} style={styles.variantCard}>
                    <Text style={styles.variantTitle}>
                      {pvg.variantGroup.name}
                      {pvg.isRequired ? <Text style={styles.required}> *</Text> : null}
                    </Text>
                    <View style={styles.swatches}>
                      {availableOptions.map((option) => {
                        const isSelected = currentSelection?.optionId === option.id;
                        return (
                          <TouchableOpacity
                            key={option.id}
                            style={[styles.swatch, isSelected && styles.swatchSelected]}
                            onPress={() => selectOption(pvg, option)}
                            activeOpacity={0.75}
                          >
                            {option.hexColor ? (
                              <View
                                style={[
                                  styles.swatchColor,
                                  { backgroundColor: option.hexColor },
                                  isSelected && styles.swatchColorSelected,
                                ]}
                              />
                            ) : (
                              <View
                                style={[
                                  styles.swatchColor,
                                  styles.swatchColorNeutral,
                                  isSelected && styles.swatchColorSelected,
                                ]}
                              >
                                <Text style={styles.swatchCodeInner}>{option.code.slice(0, 2)}</Text>
                              </View>
                            )}
                            <Text style={styles.swatchName} numberOfLines={1}>
                              {option.name}
                            </Text>
                            <Text style={styles.swatchCode}>{option.code}</Text>
                          </TouchableOpacity>
                        );
                      })}
                    </View>
                    {currentSelection && (
                      <Text style={styles.selectedLabel}>
                        Selected: {currentSelection.optionName}
                      </Text>
                    )}
                  </Card>
                );
              })}
            </View>
          )}

          {/* Quantity */}
          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Quantity</Text>
            <View style={styles.quantityRow}>
              <TouchableOpacity
                style={styles.qtyBtn}
                onPress={() => setQuantity((q) => Math.max(1, q - 1))}
              >
                <Text style={styles.qtyBtnText}>−</Text>
              </TouchableOpacity>
              <Text style={styles.qtyValue}>{quantity}</Text>
              <TouchableOpacity
                style={styles.qtyBtn}
                onPress={() => setQuantity((q) => q + 1)}
              >
                <Text style={styles.qtyBtnText}>+</Text>
              </TouchableOpacity>
            </View>
          </Card>

          {/* Customization Notes */}
          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Customization Notes (Optional)</Text>
            <TextInput
              style={styles.notesInput}
              value={notes}
              onChangeText={setNotes}
              placeholder="Add any special requests or notes..."
              placeholderTextColor={Colors.textMuted}
              multiline
              numberOfLines={3}
              textAlignVertical="top"
            />
          </Card>

          {/* Add to Cart */}
          <Button
            title="Add to Cart"
            onPress={handleAddToCart}
            loading={addToCartMutation.isPending}
            disabled={!canAddToCart}
            size="lg"
            style={styles.addButton}
          />
          {!canAddToCart && (
            <Text style={styles.requiredNote}>* Selection required for all marked groups.</Text>
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { paddingBottom: 40 },
  imageContainer: { width: '100%', aspectRatio: 1.2, backgroundColor: Colors.borderLight },
  image: { width: '100%', height: '100%' },
  imagePlaceholder: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.borderLight,
  },
  imagePlaceholderText: { color: Colors.textMuted, fontSize: 13 },
  body: { padding: 16 },
  name: { fontSize: 22, fontWeight: '800', color: Colors.text, marginBottom: 4 },
  sku: { fontSize: 13, color: Colors.textMuted, marginBottom: 2 },
  category: { fontSize: 13, color: Colors.primary, fontWeight: '600', marginBottom: 16 },
  section: { marginBottom: 14 },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 8 },
  bodyText: { fontSize: 14, color: Colors.textSecondary, lineHeight: 22 },
  variantCard: { marginBottom: 12 },
  variantTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  required: { color: Colors.error },
  swatches: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  swatch: {
    alignItems: 'center',
    width: 56,
    padding: 6,
    borderRadius: 8,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.background,
  },
  swatchSelected: { borderColor: Colors.primary, backgroundColor: Colors.primaryLight },
  swatchColor: {
    width: 32,
    height: 32,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: 'rgba(0,0,0,0.1)',
    marginBottom: 4,
  },
  swatchColorNeutral: {
    backgroundColor: Colors.borderLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  swatchColorSelected: { borderWidth: 2, borderColor: Colors.primary },
  swatchCodeInner: { fontSize: 9, color: Colors.textMuted, fontWeight: '700' },
  swatchName: { fontSize: 9, color: Colors.text, textAlign: 'center', fontWeight: '600' },
  swatchCode: { fontSize: 8, color: Colors.textMuted, textAlign: 'center' },
  selectedLabel: { marginTop: 8, fontSize: 12, color: Colors.primary, fontWeight: '600' },
  quantityRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
  },
  qtyBtn: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: Colors.primaryLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtyBtnText: { fontSize: 22, color: Colors.primary, fontWeight: '700', lineHeight: 26 },
  qtyValue: { fontSize: 20, fontWeight: '700', color: Colors.text, minWidth: 32, textAlign: 'center' },
  notesInput: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    padding: 12,
    fontSize: 14,
    color: Colors.text,
    minHeight: 80,
    backgroundColor: Colors.background,
  },
  addButton: { marginTop: 8 },
  requiredNote: { marginTop: 8, fontSize: 12, color: Colors.textMuted, textAlign: 'center' },
});

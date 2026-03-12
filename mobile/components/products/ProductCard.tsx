import React from 'react';
import { Image, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '../../constants/colors';
import { getImageUrl } from '../../lib/api';
import type { Product } from '../../lib/types';

interface ProductCardProps {
  product: Product;
  onPress: () => void;
}

export function ProductCard({ product, onPress }: ProductCardProps) {
  return (
    <TouchableOpacity style={styles.card} onPress={onPress} activeOpacity={0.8}>
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
      <View style={styles.info}>
        <Text style={styles.name} numberOfLines={2}>{product.name}</Text>
        <Text style={styles.sku}>SKU: {product.sku}</Text>
        <Text style={styles.category} numberOfLines={1}>{product.categoryName}</Text>
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
    overflow: 'hidden',
    flex: 1,
    margin: 4,
  },
  imageContainer: { aspectRatio: 1 },
  image: { width: '100%', height: '100%' },
  imagePlaceholder: {
    flex: 1,
    backgroundColor: Colors.borderLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  imagePlaceholderText: { fontSize: 11, color: Colors.textMuted },
  info: { padding: 10 },
  name: { fontSize: 13, fontWeight: '600', color: Colors.text, marginBottom: 2 },
  sku: { fontSize: 11, color: Colors.textMuted, marginBottom: 2 },
  category: { fontSize: 11, color: Colors.primary },
});

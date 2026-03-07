import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import {
  FlatList,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ProductCard } from '../../../components/products/ProductCard';
import { EmptyState } from '../../../components/ui/EmptyState';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { catalogApi } from '../../../lib/api/catalog';
import type { Category } from '../../../lib/types';

export default function ProductsScreen() {
  const router = useRouter();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const searchTimer = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  function handleSearchChange(text: string) {
    setSearchTerm(text);
    if (searchTimer.current) clearTimeout(searchTimer.current);
    searchTimer.current = setTimeout(() => setDebouncedSearch(text), 400);
  }

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: () => catalogApi.getCategories().then((r) => r.data),
    staleTime: 60_000,
  });

  const {
    data,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['products', { searchTerm: debouncedSearch, categoryId: selectedCategory }],
    queryFn: () =>
      catalogApi
        .searchProducts({
          searchTerm: debouncedSearch || undefined,
          categoryId: selectedCategory ?? undefined,
          pageSize: 40,
        })
        .then((r) => r.data),
  });

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={refetch} />;

  const products = data?.items ?? [];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <View style={styles.container}>
        {/* Search */}
        <View style={styles.searchBar}>
          <TextInput
            style={styles.searchInput}
            placeholder="Search products..."
            placeholderTextColor={Colors.textMuted}
            value={searchTerm}
            onChangeText={handleSearchChange}
            returnKeyType="search"
            clearButtonMode="while-editing"
          />
        </View>

        {/* Category Filter */}
        {categories && categories.length > 0 && (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.filterRow}
          >
            <TouchableOpacity
              style={[styles.filterChip, !selectedCategory && styles.filterChipActive]}
              onPress={() => setSelectedCategory(null)}
            >
              <Text style={[styles.filterLabel, !selectedCategory && styles.filterLabelActive]}>
                All
              </Text>
            </TouchableOpacity>
            {categories.map((cat: Category) => (
              <TouchableOpacity
                key={cat.id}
                style={[styles.filterChip, selectedCategory === cat.id && styles.filterChipActive]}
                onPress={() => setSelectedCategory(cat.id === selectedCategory ? null : cat.id)}
              >
                <Text
                  style={[
                    styles.filterLabel,
                    selectedCategory === cat.id && styles.filterLabelActive,
                  ]}
                >
                  {cat.name}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        )}

        {/* Products Grid */}
        {products.length === 0 ? (
          <EmptyState title="No products found" subtitle="Try adjusting your search or filters." />
        ) : (
          <FlatList
            data={products}
            keyExtractor={(item) => item.id}
            numColumns={2}
            contentContainerStyle={styles.grid}
            refreshControl={<RefreshControl refreshing={false} onRefresh={refetch} />}
            renderItem={({ item }) => (
              <ProductCard
                product={item}
                onPress={() => router.push(`/(tabs)/products/${item.id}` as never)}
              />
            )}
          />
        )}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  container: { flex: 1 },
  searchBar: { padding: 12, paddingBottom: 8 },
  searchInput: {
    backgroundColor: Colors.surface,
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 10,
    fontSize: 14,
    color: Colors.text,
  },
  filterRow: { paddingHorizontal: 12, paddingBottom: 8, gap: 8 },
  filterChip: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 16,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
  },
  filterChipActive: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  filterLabel: { fontSize: 13, color: Colors.textSecondary, fontWeight: '500' },
  filterLabelActive: { color: Colors.primary, fontWeight: '600' },
  grid: { padding: 8, paddingBottom: 32 },
});

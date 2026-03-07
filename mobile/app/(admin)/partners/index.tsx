import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import { FlatList, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Card } from '../../../components/ui/Card';
import { EmptyState } from '../../../components/ui/EmptyState';
import { ErrorState } from '../../../components/ui/ErrorState';
import { LoadingScreen } from '../../../components/ui/LoadingScreen';
import { Colors } from '../../../constants/colors';
import { adminApi } from '../../../lib/api/admin';

const STATUS_FILTERS = ['All', 'Pending', 'Active', 'Suspended'];

const STATUS_COLORS: Record<string, string> = {
  Pending: Colors.warning,
  Active: Colors.success,
  Suspended: Colors.error,
  Rejected: Colors.gray,
};

export default function AdminPartnersScreen() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [activeStatus, setActiveStatus] = useState('All');

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-partners', activeStatus, search],
    queryFn: () => adminApi.getPartners({
      status: activeStatus === 'All' ? undefined : activeStatus,
      searchTerm: search || undefined,
      pageSize: 50,
    }),
  });

  const partners = data?.data?.items ?? [];

  if (isLoading) return <LoadingScreen />;
  if (isError) return <ErrorState onRetry={refetch} />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <View style={styles.searchBar}>
        <TextInput
          style={styles.searchInput}
          value={search}
          onChangeText={setSearch}
          placeholder="Search partners..."
          placeholderTextColor={Colors.textMuted}
          returnKeyType="search"
        />
      </View>

      <FlatList
        horizontal
        data={STATUS_FILTERS}
        keyExtractor={(s) => s}
        showsHorizontalScrollIndicator={false}
        style={styles.filterList}
        contentContainerStyle={styles.filterContent}
        renderItem={({ item }) => (
          <TouchableOpacity
            style={[styles.chip, activeStatus === item && styles.chipActive]}
            onPress={() => setActiveStatus(item)}
          >
            <Text style={[styles.chipText, activeStatus === item && styles.chipTextActive]}>
              {item}
            </Text>
          </TouchableOpacity>
        )}
      />

      <FlatList
        data={partners}
        keyExtractor={(p) => p.id}
        contentContainerStyle={styles.list}
        ListEmptyComponent={<EmptyState title="No partners found" />}
        renderItem={({ item }) => (
          <TouchableOpacity onPress={() => router.push(`/(admin)/partners/${item.id}` as never)}>
            <Card style={styles.partnerCard}>
              <View style={styles.partnerHeader}>
                <Text style={styles.companyName}>{item.companyName}</Text>
                <View style={[styles.badge, { backgroundColor: (STATUS_COLORS[item.status] ?? Colors.gray) + '20' }]}>
                  <Text style={[styles.badgeText, { color: STATUS_COLORS[item.status] ?? Colors.gray }]}>
                    {item.status}
                  </Text>
                </View>
              </View>
              <Text style={styles.meta}>{item.email}</Text>
              <Text style={styles.meta}>{item.city}, {item.country} · {item.userCount} user{item.userCount !== 1 ? 's' : ''}</Text>
            </Card>
          </TouchableOpacity>
        )}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  searchBar: { padding: 12, paddingBottom: 0 },
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
  filterList: { maxHeight: 50, flexGrow: 0 },
  filterContent: { paddingHorizontal: 12, paddingVertical: 8, gap: 8 },
  chip: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 20,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  chipActive: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  chipText: { fontSize: 12, fontWeight: '600', color: Colors.textSecondary },
  chipTextActive: { color: '#fff' },
  list: { padding: 12, gap: 10, paddingBottom: 40 },
  partnerCard: { padding: 14 },
  partnerHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 },
  companyName: { fontSize: 15, fontWeight: '700', color: Colors.text, flex: 1, marginRight: 8 },
  badge: { borderRadius: 6, paddingHorizontal: 8, paddingVertical: 2 },
  badgeText: { fontSize: 11, fontWeight: '700' },
  meta: { fontSize: 12, color: Colors.textSecondary, marginTop: 2 },
});

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React from 'react';
import { useTranslation } from 'react-i18next';
import {
  Alert,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { ErrorState } from '../../components/ui/ErrorState';
import { LoadingScreen } from '../../components/ui/LoadingScreen';
import { Colors } from '../../constants/colors';
import { ordersApi } from '../../lib/api/orders';
import { partnersApi } from '../../lib/api/partners';
import { useAuth } from '../../lib/auth';

function InfoRow({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <View style={styles.infoRow}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value || '—'}</Text>
    </View>
  );
}

export default function ProfileScreen() {
  const { t } = useTranslation();
  const { signOut } = useAuth();
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: profile, isLoading, isError, refetch } = useQuery({
    queryKey: ['partner-profile'],
    queryFn: () => partnersApi.getProfile().then((r) => r.data),
  });

  const { data: savedAddressesRes } = useQuery({
    queryKey: ['saved-addresses'],
    queryFn: ordersApi.getSavedAddresses,
  });
  const savedAddresses = savedAddressesRes?.data ?? [];

  const deleteAddressMutation = useMutation({
    mutationFn: ordersApi.deleteSavedAddress,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['saved-addresses'] }),
  });

  function handleDeleteAddress(id: string, label: string) {
    Alert.alert('Delete Address', `Remove "${label}"?`, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: () => deleteAddressMutation.mutate(id),
      },
    ]);
  }

  function handleLogout() {
    Alert.alert('Sign Out', 'Are you sure you want to sign out?', [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Sign Out', style: 'destructive', onPress: signOut },
    ]);
  }

  if (isLoading) return <LoadingScreen />;
  if (isError || !profile) return <ErrorState onRetry={refetch} />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.content}>
        {/* Avatar */}
        <View style={styles.avatarSection}>
          <View style={styles.avatar}>
            <Text style={styles.avatarInitial}>
              {profile.firstName?.[0]?.toUpperCase() ?? '?'}
            </Text>
          </View>
          <Text style={styles.fullName}>
            {profile.firstName} {profile.lastName}
          </Text>
          <Text style={styles.email}>{profile.email}</Text>
        </View>

        {/* Personal Info */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>{t('profile.personalInfo')}</Text>
          <InfoRow label={t('profile.firstName')} value={profile.firstName} />
          <InfoRow label={t('profile.lastName')} value={profile.lastName} />
          <InfoRow label={t('common.email')} value={profile.email} />
          <InfoRow label={t('profile.phone')} value={profile.phone} />
          <InfoRow label="Role" value={profile.role} />
        </Card>

        {/* Company Info */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>{t('profile.companyInfo')}</Text>
          <InfoRow label={t('partners.company')} value={profile.company?.name} />
          <InfoRow label={t('common.status')} value={profile.company?.status} />
          <InfoRow label={t('common.address')} value={profile.company?.address} />
          <InfoRow label={t('common.phone')} value={profile.company?.phone} />
          <InfoRow label={t('common.email')} value={profile.company?.email} />
        </Card>

        {/* Saved Addresses */}
        <Card style={styles.section}>
          <Text style={styles.sectionTitle}>Saved Addresses</Text>
          {savedAddresses.length === 0 ? (
            <Text style={styles.emptyText}>No saved addresses yet. Save one during checkout.</Text>
          ) : (
            savedAddresses.map((a) => (
              <View key={a.id} style={styles.addressRow}>
                <View style={styles.addressInfo}>
                  <View style={styles.addressLabelRow}>
                    <Text style={styles.addressLabel}>{a.label}</Text>
                    {a.isDefault && (
                      <View style={styles.defaultBadge}>
                        <Text style={styles.defaultBadgeText}>Default</Text>
                      </View>
                    )}
                  </View>
                  <Text style={styles.addressDetail}>
                    {a.address}, {a.city}, {a.state} {a.postalCode}, {a.country}
                  </Text>
                </View>
                <TouchableOpacity
                  onPress={() => handleDeleteAddress(a.id, a.label)}
                  style={styles.deleteBtn}
                >
                  <Text style={styles.deleteBtnText}>✕</Text>
                </TouchableOpacity>
              </View>
            ))
          )}
        </Card>

        {/* Account Balance */}
        <TouchableOpacity
          style={styles.accountButton}
          onPress={() => router.push('/(tabs)/account')}
          activeOpacity={0.75}
        >
          <Text style={styles.accountButtonIcon}>💰</Text>
          <Text style={styles.accountButtonText}>{t('profile.myAccount')}</Text>
          <Text style={styles.accountButtonChevron}>›</Text>
        </TouchableOpacity>

        {/* Logout */}
        <Button
          title={t('profile.logout')}
          variant="danger"
          onPress={handleLogout}
          size="lg"
          style={styles.logoutButton}
        />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  content: { padding: 16, paddingBottom: 40 },
  avatarSection: { alignItems: 'center', paddingVertical: 24 },
  avatar: {
    width: 72,
    height: 72,
    borderRadius: 36,
    backgroundColor: Colors.primaryLight,
    borderWidth: 2,
    borderColor: Colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 12,
  },
  avatarInitial: { fontSize: 28, fontWeight: '800', color: Colors.primary },
  fullName: { fontSize: 20, fontWeight: '700', color: Colors.text, marginBottom: 4 },
  email: { fontSize: 14, color: Colors.textSecondary },
  section: { marginBottom: 16 },
  sectionTitle: { fontSize: 14, fontWeight: '700', color: Colors.text, marginBottom: 10 },
  infoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  infoLabel: { fontSize: 13, color: Colors.textSecondary, flex: 1 },
  infoValue: { fontSize: 13, color: Colors.text, fontWeight: '500', flex: 2, textAlign: 'right' },
  accountButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: Colors.surface,
    borderRadius: 12,
    padding: 16,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  accountButtonIcon: { fontSize: 20, marginRight: 12 },
  accountButtonText: { flex: 1, fontSize: 15, fontWeight: '600', color: Colors.primary },
  accountButtonChevron: { fontSize: 22, color: Colors.textMuted, fontWeight: '300' },
  logoutButton: { marginTop: 8 },
  emptyText: { fontSize: 13, color: Colors.textMuted, textAlign: 'center', paddingVertical: 8 },
  addressRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  addressInfo: { flex: 1 },
  addressLabelRow: { flexDirection: 'row', alignItems: 'center', marginBottom: 3 },
  addressLabel: { fontSize: 13, fontWeight: '700', color: Colors.text, marginRight: 6 },
  defaultBadge: {
    backgroundColor: Colors.primary + '20',
    borderRadius: 4,
    paddingHorizontal: 6,
    paddingVertical: 1,
  },
  defaultBadgeText: { fontSize: 10, fontWeight: '700', color: Colors.primary },
  addressDetail: { fontSize: 12, color: Colors.textSecondary },
  deleteBtn: { padding: 8 },
  deleteBtnText: { fontSize: 14, color: Colors.error, fontWeight: '700' },
});

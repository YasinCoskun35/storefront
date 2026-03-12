import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Switch,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { Colors } from '../constants/colors';
import { ordersApi } from '../lib/api/orders';
import type { SavedAddress } from '../lib/types';

interface FieldProps {
  label: string;
  value: string;
  onChangeText: (v: string) => void;
  placeholder?: string;
  required?: boolean;
  multiline?: boolean;
  keyboardType?: 'default' | 'email-address' | 'phone-pad';
}

function Field({ label, value, onChangeText, placeholder, required, multiline, keyboardType }: FieldProps) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>
        {label}
        {required ? <Text style={styles.required}> *</Text> : ''}
      </Text>
      <TextInput
        style={[styles.input, multiline && styles.inputMulti]}
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor={Colors.textMuted}
        multiline={multiline}
        numberOfLines={multiline ? 3 : 1}
        textAlignVertical={multiline ? 'top' : 'center'}
        keyboardType={keyboardType}
      />
    </View>
  );
}

export default function CheckoutScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const queryClient = useQueryClient();

  const [address, setAddress] = useState('');
  const [city, setCity] = useState('');
  const [state, setState] = useState('');
  const [postalCode, setPostalCode] = useState('');
  const [country, setCountry] = useState('');
  const [deliveryNotes, setDeliveryNotes] = useState('');
  const [requestedDate, setRequestedDate] = useState('');
  const [orderNotes, setOrderNotes] = useState('');
  const [saveAddress, setSaveAddress] = useState(false);
  const [addressLabel, setAddressLabel] = useState('');

  const { data: savedAddressesRes } = useQuery({
    queryKey: ['saved-addresses'],
    queryFn: ordersApi.getSavedAddresses,
  });
  const savedAddresses = savedAddressesRes?.data ?? [];

  function applyAddress(a: SavedAddress) {
    setAddress(a.address);
    setCity(a.city);
    setState(a.state);
    setPostalCode(a.postalCode);
    setCountry(a.country);
  }

  const saveAddressMutation = useMutation({
    mutationFn: ordersApi.createSavedAddress,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['saved-addresses'] }),
  });

  const placeOrderMutation = useMutation({
    mutationFn: ordersApi.createOrder,
    onSuccess: async (res) => {
      if (saveAddress && addressLabel.trim()) {
        await saveAddressMutation.mutateAsync({
          label: addressLabel.trim(),
          address: address.trim(),
          city: city.trim(),
          state: state.trim(),
          postalCode: postalCode.trim(),
          country: country.trim(),
          isDefault: false,
        });
      }
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['order-stats'] });
      const orderId = res.data.orderId;
      router.replace(`/(tabs)/orders/${orderId}` as never);
    },
    onError: (err: any) => {
      Alert.alert(
        'Order Failed',
        err.response?.data?.message ?? 'Failed to place order. Please try again.'
      );
    },
  });

  function validate() {
    if (!address.trim()) return 'Delivery address is required.';
    if (!city.trim()) return 'City is required.';
    if (!state.trim()) return 'State is required.';
    if (!postalCode.trim()) return 'Postal code is required.';
    if (!country.trim()) return 'Country is required.';
    if (requestedDate && !/^\d{4}-\d{2}-\d{2}$/.test(requestedDate.trim())) {
      return 'Requested delivery date must be in YYYY-MM-DD format.';
    }
    if (saveAddress && !addressLabel.trim()) return 'Please enter a label for the saved address.';
    return null;
  }

  function handlePlaceOrder() {
    const error = validate();
    if (error) {
      Alert.alert('Missing Information', error);
      return;
    }
    placeOrderMutation.mutate({
      deliveryAddress: address.trim(),
      deliveryCity: city.trim(),
      deliveryState: state.trim(),
      deliveryPostalCode: postalCode.trim(),
      deliveryCountry: country.trim(),
      deliveryNotes: deliveryNotes.trim() || undefined,
      requestedDeliveryDate: requestedDate.trim() || undefined,
      notes: orderNotes.trim() || undefined,
    });
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">

          {/* Saved addresses picker */}
          {savedAddresses.length > 0 && (
            <Card style={styles.section}>
              <Text style={styles.sectionTitle}>Saved Addresses</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.addressScroll}>
                {savedAddresses.map((a) => (
                  <TouchableOpacity
                    key={a.id}
                    style={[styles.addressChip, a.isDefault && styles.addressChipDefault]}
                    onPress={() => applyAddress(a)}
                  >
                    <Text style={styles.addressChipLabel}>{a.label}</Text>
                    <Text style={styles.addressChipDetail}>{a.city}, {a.country}</Text>
                  </TouchableOpacity>
                ))}
              </ScrollView>
            </Card>
          )}

          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>{t('checkout.deliveryAddress')}</Text>
            <Field label={t('checkout.streetAddress')} value={address} onChangeText={setAddress} required placeholder="123 Main Street" />
            <Field label={t('checkout.city')} value={city} onChangeText={setCity} required placeholder="Istanbul" />
            <Field label={t('checkout.state')} value={state} onChangeText={setState} required placeholder="Istanbul" />
            <Field label={t('checkout.postalCode')} value={postalCode} onChangeText={setPostalCode} required placeholder="34000" />
            <Field label={t('checkout.country')} value={country} onChangeText={setCountry} required placeholder="Turkey" />

            {/* Save address toggle */}
            <View style={styles.saveRow}>
              <Text style={styles.saveLabel}>Save this address</Text>
              <Switch
                value={saveAddress}
                onValueChange={setSaveAddress}
                trackColor={{ true: Colors.primary }}
              />
            </View>
            {saveAddress && (
              <Field
                label="Address Label"
                value={addressLabel}
                onChangeText={setAddressLabel}
                placeholder="e.g. Office, Warehouse"
                required
              />
            )}
          </Card>

          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Delivery Options (Optional)</Text>
            <Field
              label={t('checkout.deliveryNotes')}
              value={deliveryNotes}
              onChangeText={setDeliveryNotes}
              placeholder="Special instructions for delivery..."
              multiline
            />
            <Field
              label={t('checkout.requestedDeliveryDate')}
              value={requestedDate}
              onChangeText={setRequestedDate}
              placeholder="YYYY-MM-DD"
              keyboardType="phone-pad"
            />
          </Card>

          <Card style={styles.section}>
            <Text style={styles.sectionTitle}>Order Notes (Optional)</Text>
            <Field
              label={t('checkout.additionalNotes')}
              value={orderNotes}
              onChangeText={setOrderNotes}
              placeholder="Additional notes for this order..."
              multiline
            />
          </Card>

          <Button
            title={placeOrderMutation.isPending ? t('checkout.submitting') : t('checkout.submitOrder')}
            onPress={handlePlaceOrder}
            loading={placeOrderMutation.isPending}
            size="lg"
            style={styles.submitButton}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  flex: { flex: 1 },
  content: { padding: 16, paddingBottom: 40 },
  section: { marginBottom: 16 },
  sectionTitle: { fontSize: 15, fontWeight: '700', color: Colors.text, marginBottom: 12 },
  field: { marginBottom: 12 },
  label: { fontSize: 13, fontWeight: '600', color: Colors.text, marginBottom: 5 },
  required: { color: Colors.error },
  input: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 10,
    fontSize: 14,
    color: Colors.text,
    backgroundColor: Colors.background,
  },
  inputMulti: { minHeight: 72, paddingTop: 10 },
  submitButton: { marginTop: 8 },
  addressScroll: { marginBottom: 4 },
  addressChip: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 10,
    marginRight: 10,
    minWidth: 110,
  },
  addressChipDefault: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primary + '12',
  },
  addressChipLabel: { fontSize: 13, fontWeight: '700', color: Colors.text, marginBottom: 2 },
  addressChipDetail: { fontSize: 11, color: Colors.textMuted },
  saveRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 12,
    marginTop: 4,
  },
  saveLabel: { fontSize: 14, fontWeight: '600', color: Colors.text },
});

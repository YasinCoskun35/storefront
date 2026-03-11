import { Tabs } from 'expo-router';
import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { Colors } from '../../constants/colors';
import { useQuery } from '@tanstack/react-query';
import { ordersApi } from '../../lib/api/orders';

function TabIcon({ icon, focused }: { icon: string; focused: boolean }) {
  return (
    <Text style={{ fontSize: 22, opacity: focused ? 1 : 0.5 }}>{icon}</Text>
  );
}

function CartTabIcon({ focused }: { focused: boolean }) {
  const { data } = useQuery({
    queryKey: ['cart'],
    queryFn: () => ordersApi.getCart().then((r) => r.data),
    staleTime: 15_000,
  });

  const count = data?.itemCount ?? 0;

  return (
    <View>
      <Text style={{ fontSize: 22, opacity: focused ? 1 : 0.5 }}>🛒</Text>
      {count > 0 && (
        <View style={styles.badge}>
          <Text style={styles.badgeText}>{count > 99 ? '99+' : count}</Text>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    position: 'absolute',
    top: -4,
    right: -8,
    backgroundColor: Colors.error,
    borderRadius: 8,
    minWidth: 16,
    height: 16,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 3,
  },
  badgeText: { color: '#fff', fontSize: 9, fontWeight: '700' },
});

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: Colors.primary,
        tabBarInactiveTintColor: Colors.textMuted,
        tabBarStyle: {
          borderTopWidth: 1,
          borderTopColor: Colors.border,
          backgroundColor: Colors.surface,
          paddingBottom: 4,
        },
        tabBarLabelStyle: { fontSize: 10, fontWeight: '600' },
        headerStyle: { backgroundColor: Colors.surface },
        headerTitleStyle: { fontWeight: '700', fontSize: 17 },
        headerShadowVisible: false,
        headerTintColor: Colors.text,
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Home',
          tabBarIcon: ({ focused }) => <TabIcon icon="🏠" focused={focused} />,
          headerTitle: 'Dashboard',
        }}
      />
      <Tabs.Screen
        name="products/index"
        options={{
          title: 'Products',
          tabBarIcon: ({ focused }) => <TabIcon icon="📦" focused={focused} />,
          headerTitle: 'Products',
        }}
      />
      <Tabs.Screen
        name="cart"
        options={{
          title: 'Cart',
          tabBarIcon: ({ focused }) => <CartTabIcon focused={focused} />,
          headerTitle: 'Cart',
        }}
      />
      <Tabs.Screen
        name="orders/index"
        options={{
          title: 'Orders',
          tabBarIcon: ({ focused }) => <TabIcon icon="📋" focused={focused} />,
          headerTitle: 'My Orders',
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Profile',
          tabBarIcon: ({ focused }) => <TabIcon icon="👤" focused={focused} />,
          headerTitle: 'Profile',
        }}
      />
      {/* Hidden from tab bar but accessible via router */}
      <Tabs.Screen name="products/[id]" options={{ href: null, headerTitle: 'Product Details' }} />
      <Tabs.Screen name="orders/[id]" options={{ href: null, headerTitle: 'Order Details' }} />
      <Tabs.Screen name="account" options={{ href: null, headerTitle: 'Cari Hesap' }} />
    </Tabs>
  );
}

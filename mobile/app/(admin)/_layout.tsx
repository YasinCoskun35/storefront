import { Tabs } from 'expo-router';
import React from 'react';
import { Text } from 'react-native';
import { Colors } from '../../constants/colors';

function TabIcon({ icon, focused }: { icon: string; focused: boolean }) {
  return <Text style={{ fontSize: 22, opacity: focused ? 1 : 0.5 }}>{icon}</Text>;
}

export default function AdminLayout() {
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
          title: 'Dashboard',
          tabBarIcon: ({ focused }) => <TabIcon icon="📊" focused={focused} />,
          headerTitle: 'Admin Dashboard',
        }}
      />
      <Tabs.Screen
        name="orders/index"
        options={{
          title: 'Orders',
          tabBarIcon: ({ focused }) => <TabIcon icon="📋" focused={focused} />,
          headerTitle: 'All Orders',
        }}
      />
      <Tabs.Screen
        name="partners/index"
        options={{
          title: 'Partners',
          tabBarIcon: ({ focused }) => <TabIcon icon="🏢" focused={focused} />,
          headerTitle: 'Partners',
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Account',
          tabBarIcon: ({ focused }) => <TabIcon icon="👤" focused={focused} />,
          headerTitle: 'Account',
        }}
      />
      {/* Hidden screens */}
      <Tabs.Screen name="orders/[id]" options={{ href: null, headerTitle: 'Order Details' }} />
      <Tabs.Screen name="partners/[id]" options={{ href: null, headerTitle: 'Partner Details' }} />
    </Tabs>
  );
}

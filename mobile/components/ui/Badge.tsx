import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

interface BadgeProps {
  label: string;
  color: string;
  backgroundColor: string;
}

export function Badge({ label, color, backgroundColor }: BadgeProps) {
  return (
    <View style={[styles.container, { backgroundColor }]}>
      <Text style={[styles.text, { color }]}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 12,
    alignSelf: 'flex-start',
  },
  text: {
    fontSize: 11,
    fontWeight: '600',
    letterSpacing: 0.2,
  },
});

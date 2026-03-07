import React from 'react';
import { StyleSheet, View, type ViewProps } from 'react-native';
import { Colors } from '../../constants/colors';

interface CardProps extends ViewProps {
  children: React.ReactNode;
  padding?: number;
}

export function Card({ children, padding = 16, style, ...rest }: CardProps) {
  return (
    <View style={[styles.card, { padding }, style]} {...rest}>
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: Colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: Colors.border,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 1,
  },
});

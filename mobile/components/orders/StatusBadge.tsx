import React from 'react';
import { Badge } from '../ui/Badge';
import { getStatusConfig } from '../../lib/orderStatus';

interface StatusBadgeProps {
  status: string;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const config = getStatusConfig(status);
  return <Badge label={config.label} color={config.color} backgroundColor={config.backgroundColor} />;
}

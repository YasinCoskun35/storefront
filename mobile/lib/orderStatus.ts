import { Colors } from '../constants/colors';
import type { OrderStatus } from './types';

export interface StatusConfig {
  label: string;
  color: string;
  backgroundColor: string;
}

const STATUS_MAP: Record<OrderStatus, StatusConfig> = {
  Pending: { label: 'Pending Review', color: Colors.statusPending, backgroundColor: Colors.statusPendingBg },
  QuoteSent: { label: 'Quote Sent', color: Colors.statusQuoteSent, backgroundColor: Colors.statusQuoteSentBg },
  Confirmed: { label: 'Confirmed', color: Colors.statusConfirmed, backgroundColor: Colors.statusConfirmedBg },
  InProduction: { label: 'In Production', color: Colors.statusInProduction, backgroundColor: Colors.statusInProductionBg },
  ReadyToShip: { label: 'Ready to Ship', color: Colors.statusReadyToShip, backgroundColor: Colors.statusReadyToShipBg },
  Shipping: { label: 'Shipping', color: Colors.statusShipping, backgroundColor: Colors.statusShippingBg },
  Delivered: { label: 'Delivered', color: Colors.statusDelivered, backgroundColor: Colors.statusDeliveredBg },
  Completed: { label: 'Completed', color: Colors.statusCompleted, backgroundColor: Colors.statusCompletedBg },
  Cancelled: { label: 'Cancelled', color: Colors.statusCancelled, backgroundColor: Colors.statusCancelledBg },
};

export function getStatusConfig(status: OrderStatus | string): StatusConfig {
  return STATUS_MAP[status as OrderStatus] ?? {
    label: status,
    color: Colors.gray,
    backgroundColor: Colors.grayLight,
  };
}

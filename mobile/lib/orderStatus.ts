import { Colors } from '../constants/colors';
import type { OrderStatus } from './types';

export interface StatusConfig {
  label: string;
  color: string;
  backgroundColor: string;
}

const STATUS_MAP: Record<OrderStatus, StatusConfig> = {
  Pending: { label: 'Beklemede', color: Colors.statusPending, backgroundColor: Colors.statusPendingBg },
  QuoteSent: { label: 'Teklif Gönderildi', color: Colors.statusQuoteSent, backgroundColor: Colors.statusQuoteSentBg },
  Confirmed: { label: 'Onaylandı', color: Colors.statusConfirmed, backgroundColor: Colors.statusConfirmedBg },
  InProduction: { label: 'Üretimde', color: Colors.statusInProduction, backgroundColor: Colors.statusInProductionBg },
  ReadyToShip: { label: 'Gönderilmeye Hazır', color: Colors.statusReadyToShip, backgroundColor: Colors.statusReadyToShipBg },
  Shipping: { label: 'Kargoda', color: Colors.statusShipping, backgroundColor: Colors.statusShippingBg },
  Delivered: { label: 'Teslim Edildi', color: Colors.statusDelivered, backgroundColor: Colors.statusDeliveredBg },
  Completed: { label: 'Tamamlandı', color: Colors.statusCompleted, backgroundColor: Colors.statusCompletedBg },
  Cancelled: { label: 'İptal Edildi', color: Colors.statusCancelled, backgroundColor: Colors.statusCancelledBg },
  PendingPayment: { label: 'Ödeme Bekleniyor', color: Colors.statusPendingPayment, backgroundColor: Colors.statusPendingPaymentBg },
};

export function getStatusConfig(status: OrderStatus | string): StatusConfig {
  return STATUS_MAP[status as OrderStatus] ?? {
    label: status,
    color: Colors.gray,
    backgroundColor: Colors.grayLight,
  };
}

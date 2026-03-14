'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Building2, CreditCard, TrendingDown, TrendingUp, Minus, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { partnerPaymentApi, type PartnerAccountTransactionDto } from '@/lib/api/partners';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';

const TYPE_LABELS: Record<string, string> = {
  OrderDebit: 'Sipariş Borcu',
  PaymentCredit: 'Ödeme',
  ManualAdjustment: 'Manuel Düzeltme',
};

const METHOD_LABELS: Record<string, string> = {
  Cash: 'Nakit',
  Check: 'Çek',
  PromissoryNote: 'Senet',
  BankTransfer: 'Banka Havalesi',
};

function TransactionRow({ tx }: { tx: PartnerAccountTransactionDto }) {
  const isDebit = tx.type === 'OrderDebit';
  const isCredit = tx.type === 'PaymentCredit';

  return (
    <div className="flex items-center gap-4 py-3 border-b last:border-0">
      <div className={cn(
        'flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center',
        isCredit ? 'bg-green-100' : 'bg-red-100'
      )}>
        {isCredit
          ? <TrendingDown className="w-4 h-4 text-green-600" />
          : <TrendingUp className="w-4 h-4 text-red-600" />
        }
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="text-sm font-medium">{TYPE_LABELS[tx.type] ?? tx.type}</span>
          {tx.paymentMethod && (
            <Badge variant="outline" className="text-xs">
              {METHOD_LABELS[tx.paymentMethod] ?? tx.paymentMethod}
            </Badge>
          )}
        </div>
        <div className="flex items-center gap-2 text-xs text-gray-500 mt-0.5 flex-wrap">
          {tx.orderReference && <span>Sipariş: {tx.orderReference}</span>}
          {tx.notes && <span className="truncate max-w-xs">{tx.notes}</span>}
          <span>{new Date(tx.createdAt).toLocaleDateString('tr-TR')}</span>
        </div>
      </div>
      <span className={cn(
        'text-sm font-bold flex-shrink-0',
        isCredit ? 'text-green-600' : 'text-red-600'
      )}>
        {isCredit ? '-' : '+'}₺{tx.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
      </span>
    </div>
  );
}

export default function PartnerAccountPage() {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [amount, setAmount] = useState('');
  const [paying, setPaying] = useState(false);

  const { data: account, isLoading } = useQuery({
    queryKey: ['partner-account'],
    queryFn: () => partnerPaymentApi.getAccount(),
  });

  const balance = account?.currentBalance ?? 0;
  const balancePositive = balance > 0;
  const balanceZero = balance === 0;

  const handlePay = async () => {
    const parsed = parseFloat(amount.replace(',', '.'));
    if (!parsed || parsed <= 0) {
      toast.error('Geçerli bir tutar girin.');
      return;
    }
    setPaying(true);
    try {
      const { checkoutFormContent } = await partnerPaymentApi.initialize(parsed);
      // Store form content, redirect to payments page to render it
      sessionStorage.setItem('iyzico_form', checkoutFormContent);
      sessionStorage.setItem('iyzico_amount', parsed.toString());
      router.push('/partner/payments');
    } catch {
      toast.error('Ödeme başlatılamadı. Lütfen tekrar deneyin.');
      setPaying(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="container mx-auto px-4 py-4 flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Geri
          </Button>
          <div className="flex items-center gap-2">
            <Building2 className="h-5 w-5 text-blue-600" />
            <h1 className="text-lg font-bold">Cari Hesap</h1>
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-8 max-w-2xl">
        {/* Balance card */}
        <Card className="mb-6">
          <CardContent className="pt-6 pb-6 flex flex-col items-center text-center">
            <CreditCard className="h-10 w-10 text-gray-400 mb-3" />
            <p className="text-sm text-gray-500 mb-1">Güncel Bakiye</p>
            <p className={cn(
              'text-4xl font-extrabold mb-2',
              balanceZero ? 'text-gray-500' : balancePositive ? 'text-red-600' : 'text-green-600'
            )}>
              ₺{Math.abs(balance).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
            </p>
            <Badge className={cn(
              'text-sm',
              balanceZero ? 'bg-gray-100 text-gray-600' :
              balancePositive ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'
            )}>
              {balanceZero ? 'Borç Yok' : balancePositive ? 'Borçlu' : 'Alacaklı'}
            </Badge>
            {account && account.discountRate > 0 && (
              <p className="text-sm text-blue-600 font-medium mt-3">
                İskonto Oranı: %{account.discountRate}
              </p>
            )}
            {balancePositive && (
              <Button
                className="mt-5 bg-blue-600 hover:bg-blue-700"
                onClick={() => setDialogOpen(true)}
              >
                <CreditCard className="h-4 w-4 mr-2" />
                Ödeme Yap
              </Button>
            )}
          </CardContent>
        </Card>

        {/* Transactions */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-base">İşlem Geçmişi</CardTitle>
          </CardHeader>
          <CardContent>
            {!account?.transactions?.length ? (
              <div className="flex flex-col items-center py-10 text-gray-400">
                <Minus className="h-8 w-8 mb-2" />
                <p className="text-sm">Henüz işlem bulunmuyor.</p>
              </div>
            ) : (
              account.transactions
                .slice()
                .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                .map((tx) => <TransactionRow key={tx.id} tx={tx} />)
            )}
          </CardContent>
        </Card>
      </div>

      {/* Payment dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Ödeme Yap</DialogTitle>
          </DialogHeader>
          <div className="py-2">
            <Label htmlFor="amount" className="mb-2 block text-sm">
              Ödeme Tutarı (₺)
            </Label>
            <Input
              id="amount"
              type="number"
              min="1"
              step="0.01"
              placeholder="0.00"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="text-lg"
            />
            {balance > 0 && (
              <Button
                variant="ghost"
                size="sm"
                className="mt-2 text-blue-600 px-0"
                onClick={() => setAmount(balance.toString())}
              >
                Tüm borcu öde (₺{balance.toLocaleString('tr-TR', { minimumFractionDigits: 2 })})
              </Button>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>İptal</Button>
            <Button
              className="bg-blue-600 hover:bg-blue-700"
              onClick={handlePay}
              disabled={paying || !amount}
            >
              {paying ? 'Yönlendiriliyor...' : 'Ödemeye Devam Et'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

'use client';

import { useSearchParams, useRouter } from 'next/navigation';
import { Suspense } from 'react';
import { CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';

function ResultContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const status = searchParams.get('status');
  const amountRaw = searchParams.get('amount');
  const amount = amountRaw ? parseFloat(amountRaw) : null;

  const isSuccess = status === 'success';

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 p-8">
      <div className="bg-white rounded-2xl shadow-sm border p-10 max-w-sm w-full text-center">
        {isSuccess ? (
          <>
            <CheckCircle className="h-16 w-16 text-green-500 mx-auto mb-4" />
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Ödeme Başarılı</h1>
            {amount !== null && (
              <p className="text-gray-500 mb-6">
                ₺{amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} tutarındaki
                ödemeniz hesabınıza yansıtıldı.
              </p>
            )}
          </>
        ) : (
          <>
            <XCircle className="h-16 w-16 text-red-500 mx-auto mb-4" />
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Ödeme Başarısız</h1>
            <p className="text-gray-500 mb-6">
              Ödeme işlemi gerçekleştirilemedi. Lütfen tekrar deneyin.
            </p>
          </>
        )}
        <Button
          className="w-full bg-blue-600 hover:bg-blue-700"
          onClick={() => router.push('/partner/account')}
        >
          Cari Hesaba Dön
        </Button>
      </div>
    </div>
  );
}

export default function PaymentResultPage() {
  return (
    <Suspense>
      <ResultContent />
    </Suspense>
  );
}

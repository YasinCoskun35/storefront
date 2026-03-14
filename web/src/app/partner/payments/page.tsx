'use client';

import { useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function PartnerPaymentsPage() {
  const router = useRouter();
  const containerRef = useRef<HTMLDivElement>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const formContent = sessionStorage.getItem('iyzico_form');
    if (!formContent) {
      setError('Ödeme formu bulunamadı. Lütfen tekrar deneyin.');
      return;
    }

    sessionStorage.removeItem('iyzico_form');
    sessionStorage.removeItem('iyzico_amount');

    const container = containerRef.current;
    if (!container) return;

    // Inject iyzico form HTML — it contains a <script> that creates the iframe
    container.innerHTML = formContent;

    // Re-execute any scripts (innerHTML does not run scripts)
    container.querySelectorAll('script').forEach((oldScript) => {
      const newScript = document.createElement('script');
      Array.from(oldScript.attributes).forEach((attr) =>
        newScript.setAttribute(attr.name, attr.value)
      );
      newScript.textContent = oldScript.textContent;
      oldScript.parentNode?.replaceChild(newScript, oldScript);
    });
  }, []);

  if (error) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-4 p-8 text-center">
        <p className="text-red-600 font-medium">{error}</p>
        <button
          className="text-blue-600 underline text-sm"
          onClick={() => router.push('/partner/account')}
        >
          Geri dön
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50">
      <div id="iyzipay-checkout-form" className="popup" ref={containerRef} />
    </div>
  );
}

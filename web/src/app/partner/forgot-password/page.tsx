'use client';

import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { useTranslations } from 'next-intl';
import { partnerPublicApi } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { KeyRound, MailCheck } from 'lucide-react';

interface ForgotPasswordForm {
  email: string;
}

export default function PartnerForgotPasswordPage() {
  const t = useTranslations('auth');
  const [sent, setSent] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordForm>();

  const mutation = useMutation({
    mutationFn: (data: ForgotPasswordForm) => partnerPublicApi.forgotPassword(data.email),
    onSuccess: () => setSent(true),
    // Backend always returns 200; treat any network error the same to avoid leaking info
    onError: () => setSent(true),
  });

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <KeyRound className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold">{t('forgotPasswordTitle')}</h1>
          <p className="text-gray-600 mt-2">{t('forgotPasswordDesc')}</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>{t('forgotPasswordTitle')}</CardTitle>
            <CardDescription>{t('forgotPasswordCardDesc')}</CardDescription>
          </CardHeader>
          <CardContent>
            {sent ? (
              <div className="text-center py-6">
                <MailCheck className="h-12 w-12 text-green-600 mx-auto mb-4" />
                <p className="text-gray-700">{t('resetEmailSent')}</p>
              </div>
            ) : (
              <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className="space-y-4">
                <div>
                  <Label htmlFor="email">{t('email')}</Label>
                  <Input
                    id="email"
                    type="email"
                    {...register('email', {
                      required: t('emailRequired'),
                      pattern: {
                        value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                        message: t('invalidEmail'),
                      },
                    })}
                    placeholder={t('emailPlaceholder')}
                    autoComplete="email"
                  />
                  {errors.email && (
                    <p className="text-sm text-red-600 mt-1">{errors.email.message}</p>
                  )}
                </div>

                <Button type="submit" className="w-full" disabled={mutation.isPending}>
                  {mutation.isPending ? t('sendingResetLink') : t('sendResetLink')}
                </Button>
              </form>
            )}
          </CardContent>
        </Card>

        <div className="text-center mt-4">
          <a href="/partner/login" className="text-sm text-blue-600 hover:underline">
            {t('backToLogin')}
          </a>
        </div>
      </div>
    </div>
  );
}

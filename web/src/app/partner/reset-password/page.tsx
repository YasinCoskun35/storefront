'use client';

import { Suspense, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { useTranslations } from 'next-intl';
import { partnerPublicApi } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { Eye, EyeOff, LockKeyhole } from 'lucide-react';

interface ResetPasswordForm {
  newPassword: string;
  confirmPassword: string;
}

function ResetPasswordContent() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const [showPassword, setShowPassword] = useState(false);

  const token = searchParams.get('token') ?? '';

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<ResetPasswordForm>();

  const mutation = useMutation({
    mutationFn: (data: ResetPasswordForm) =>
      partnerPublicApi.resetPassword(token, data.newPassword),
    onSuccess: () => {
      toast({ title: t('passwordResetSuccess'), description: t('passwordResetSuccessDesc') });
      router.push('/partner/login');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || t('passwordResetFailed');
      toast({ title: t('passwordResetFailed'), description: message, variant: 'destructive' });
    },
  });

  if (!token) {
    return (
      <Card>
        <CardContent className="py-8 text-center">
          <p className="text-gray-700">{t('invalidResetLink')}</p>
          <a href="/partner/forgot-password" className="text-blue-600 hover:underline text-sm mt-2 inline-block">
            {t('requestNewLink')}
          </a>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('resetPasswordTitle')}</CardTitle>
        <CardDescription>{t('resetPasswordDesc')}</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className="space-y-4">
          <div>
            <Label htmlFor="newPassword">{t('newPassword')}</Label>
            <div className="relative">
              <Input
                id="newPassword"
                type={showPassword ? 'text' : 'password'}
                {...register('newPassword', {
                  required: t('passwordRequired'),
                  minLength: { value: 8, message: t('passwordMinLength') },
                })}
                placeholder="••••••••"
                autoComplete="new-password"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
              >
                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
            {errors.newPassword && (
              <p className="text-sm text-red-600 mt-1">{errors.newPassword.message}</p>
            )}
          </div>

          <div>
            <Label htmlFor="confirmPassword">{t('confirmPassword')}</Label>
            <Input
              id="confirmPassword"
              type={showPassword ? 'text' : 'password'}
              {...register('confirmPassword', {
                required: t('passwordRequired'),
                validate: (value) =>
                  value === watch('newPassword') || t('passwordsDoNotMatch'),
              })}
              placeholder="••••••••"
              autoComplete="new-password"
            />
            {errors.confirmPassword && (
              <p className="text-sm text-red-600 mt-1">{errors.confirmPassword.message}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={mutation.isPending}>
            {mutation.isPending ? t('resettingPassword') : t('resetPasswordButton')}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

export default function PartnerResetPasswordPage() {
  const t = useTranslations('auth');

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <LockKeyhole className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold">{t('resetPasswordTitle')}</h1>
        </div>

        <Suspense fallback={null}>
          <ResetPasswordContent />
        </Suspense>

        <div className="text-center mt-4">
          <a href="/partner/login" className="text-sm text-blue-600 hover:underline">
            {t('backToLogin')}
          </a>
        </div>
      </div>
    </div>
  );
}

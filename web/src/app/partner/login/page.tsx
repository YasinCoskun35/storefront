'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { useTranslations } from 'next-intl';
import { partnerPublicApi, PartnerLoginData } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { Building2, Eye, EyeOff } from 'lucide-react';

export default function PartnerLoginPage() {
  const t = useTranslations('auth');
  const router = useRouter();
  const { toast } = useToast();
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PartnerLoginData>();

  const loginMutation = useMutation({
    mutationFn: (data: PartnerLoginData) => partnerPublicApi.login(data),
    onSuccess: (response) => {
      localStorage.setItem('partner_access_token', response.accessToken);
      localStorage.setItem('partner_refresh_token', response.refreshToken);
      localStorage.setItem('partner_user', JSON.stringify(response.user));

      toast({
        title: t('welcomeBack'),
        description: `${response.user.firstName} ${response.user.lastName} ${t('loggedInAs')}`,
      });

      router.push('/partner/dashboard');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || t('invalidCredentials');
      toast({
        title: t('loginFailed'),
        description: message,
        variant: 'destructive',
      });
    },
  });

  const onSubmit = (data: PartnerLoginData) => {
    loginMutation.mutate(data);
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <Building2 className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold">{t('partnerPortal')}</h1>
          <p className="text-gray-600 mt-2">{t('signIn')}</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>{t('partnerLoginTitle')}</CardTitle>
            <CardDescription>{t('partnerLoginDesc')}</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
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

              <div>
                <Label htmlFor="password">{t('password')}</Label>
                <div className="relative">
                  <Input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    {...register('password', {
                      required: t('passwordRequired'),
                    })}
                    placeholder="••••••••"
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                {errors.password && (
                  <p className="text-sm text-red-600 mt-1">{errors.password.message}</p>
                )}
              </div>

              <div className="flex items-center justify-between text-sm">
                <label className="flex items-center">
                  <input type="checkbox" className="mr-2" />
                  <span>{t('rememberMe')}</span>
                </label>
                <a href="/partner/forgot-password" className="text-blue-600 hover:underline">
                  {t('forgotPassword')}
                </a>
              </div>

              <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
                {loginMutation.isPending ? t('signingIn') : t('signIn')}
              </Button>
            </form>
          </CardContent>
        </Card>

        <div className="text-center mt-4">
          <p className="text-sm text-gray-600">
            {t('needHelp')}{' '}
            <a href="mailto:support@storefront.com" className="text-blue-600 hover:underline">
              {t('contactSupport')}
            </a>
          </p>
          <p className="text-xs text-gray-500 mt-2">
            {t('partnerAccountInfo')}
          </p>
        </div>
      </div>
    </div>
  );
}

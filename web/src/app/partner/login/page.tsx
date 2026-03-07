'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { partnerPublicApi, PartnerLoginData } from '@/lib/api/partners';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { Building2, Eye, EyeOff } from 'lucide-react';

export default function PartnerLoginPage() {
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
      // Store tokens
      localStorage.setItem('partner_access_token', response.accessToken);
      localStorage.setItem('partner_refresh_token', response.refreshToken);
      localStorage.setItem('partner_user', JSON.stringify(response.user));

      toast({
        title: 'Welcome back!',
        description: `Logged in as ${response.user.firstName} ${response.user.lastName}`,
      });

      // Redirect to partner dashboard
      router.push('/partner/dashboard');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Invalid email or password';
      toast({
        title: 'Login Failed',
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
        {/* Logo/Brand */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <Building2 className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold">Partner Portal</h1>
          <p className="text-gray-600 mt-2">Sign in to your account</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Welcome Back</CardTitle>
            <CardDescription>Enter your credentials to access your partner account</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div>
                <Label htmlFor="email">Email Address</Label>
                <Input
                  id="email"
                  type="email"
                  {...register('email', {
                    required: 'Email is required',
                    pattern: {
                      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                      message: 'Invalid email address',
                    },
                  })}
                  placeholder="john@company.com"
                  autoComplete="email"
                />
                {errors.email && (
                  <p className="text-sm text-red-600 mt-1">{errors.email.message}</p>
                )}
              </div>

              <div>
                <Label htmlFor="password">Password</Label>
                <div className="relative">
                  <Input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    {...register('password', {
                      required: 'Password is required',
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
                  <span>Remember me</span>
                </label>
                <a href="/partner/forgot-password" className="text-blue-600 hover:underline">
                  Forgot password?
                </a>
              </div>

              <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
                {loginMutation.isPending ? 'Signing in...' : 'Sign In'}
              </Button>
            </form>
          </CardContent>
        </Card>

        {/* Help */}
        <div className="text-center mt-4">
          <p className="text-sm text-gray-600">
            Need help?{' '}
            <a href="mailto:support@storefront.com" className="text-blue-600 hover:underline">
              Contact Support
            </a>
          </p>
          <p className="text-xs text-gray-500 mt-2">
            New partner accounts are created by our admin team
          </p>
        </div>
      </div>
    </div>
  );
}

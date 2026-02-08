'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/hooks/use-toast';
import { ArrowLeft, Building2, User } from 'lucide-react';
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api';

interface CreatePartnerForm {
  // Company
  companyName: string;
  taxId: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  industry?: string;
  website?: string;
  employeeCount?: number;
  annualRevenue?: number;
  // Admin User
  adminUser: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
  };
}

export default function NewPartnerPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [activeTab, setActiveTab] = useState('company');

  const {
    register,
    handleSubmit,
    formState: { errors },
    trigger,
  } = useForm<CreatePartnerForm>();

  const createMutation = useMutation({
    mutationFn: async (data: CreatePartnerForm) => {
      const response = await axios.post(`${API_URL}/admin/partners`, data);
      return response.data;
    },
    onSuccess: (data) => {
      toast({
        title: 'Partner Created',
        description: 'The partner company has been created successfully.',
      });
      router.push(`/admin/partners/${data.id}`);
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.response?.data?.message || 'Failed to create partner. Please try again.',
        variant: 'destructive',
      });
    },
  });

  const onNext = async () => {
    const isValid = await trigger([
      'companyName',
      'taxId',
      'email',
      'phone',
      'address',
      'city',
      'state',
      'postalCode',
      'country',
    ] as any);

    if (isValid) {
      setActiveTab('admin');
    }
  };

  const onSubmit = (data: CreatePartnerForm) => {
    createMutation.mutate(data);
  };

  return (
    <div className="container mx-auto py-8 px-4 max-w-4xl">
      {/* Header */}
      <div className="mb-6">
        <Link href="/admin/partners">
          <Button variant="ghost" size="sm" className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Partners
          </Button>
        </Link>
        <h1 className="text-3xl font-bold">Create New Partner</h1>
        <p className="text-gray-600 mt-2">Add a new B2B partner company to the system</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="company">
              <Building2 className="h-4 w-4 mr-2" />
              Company Info
            </TabsTrigger>
            <TabsTrigger value="admin">
              <User className="h-4 w-4 mr-2" />
              Admin User
            </TabsTrigger>
          </TabsList>

          {/* Company Information Tab */}
          <TabsContent value="company">
            <Card>
              <CardHeader>
                <CardTitle>Company Information</CardTitle>
                <CardDescription>Enter the partner company details</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="companyName">Company Name *</Label>
                    <Input
                      id="companyName"
                      {...register('companyName', { required: 'Company name is required' })}
                      placeholder="ABC Furniture Store"
                    />
                    {errors.companyName && (
                      <p className="text-sm text-red-600 mt-1">{errors.companyName.message}</p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="taxId">Tax ID / Business Number *</Label>
                    <Input
                      id="taxId"
                      {...register('taxId', { required: 'Tax ID is required' })}
                      placeholder="12-3456789"
                    />
                    {errors.taxId && (
                      <p className="text-sm text-red-600 mt-1">{errors.taxId.message}</p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="email">Company Email *</Label>
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
                      placeholder="info@company.com"
                    />
                    {errors.email && (
                      <p className="text-sm text-red-600 mt-1">{errors.email.message}</p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="phone">Phone Number *</Label>
                    <Input
                      id="phone"
                      {...register('phone', { required: 'Phone number is required' })}
                      placeholder="+1-555-0100"
                    />
                    {errors.phone && (
                      <p className="text-sm text-red-600 mt-1">{errors.phone.message}</p>
                    )}
                  </div>
                </div>

                <div>
                  <Label htmlFor="address">Street Address *</Label>
                  <Input
                    id="address"
                    {...register('address', { required: 'Address is required' })}
                    placeholder="123 Main Street"
                  />
                  {errors.address && (
                    <p className="text-sm text-red-600 mt-1">{errors.address.message}</p>
                  )}
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <Label htmlFor="city">City *</Label>
                    <Input
                      id="city"
                      {...register('city', { required: 'City is required' })}
                      placeholder="New York"
                    />
                    {errors.city && (
                      <p className="text-sm text-red-600 mt-1">{errors.city.message}</p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="state">State / Province *</Label>
                    <Input
                      id="state"
                      {...register('state', { required: 'State is required' })}
                      placeholder="NY"
                    />
                    {errors.state && (
                      <p className="text-sm text-red-600 mt-1">{errors.state.message}</p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="postalCode">Postal Code *</Label>
                    <Input
                      id="postalCode"
                      {...register('postalCode', { required: 'Postal code is required' })}
                      placeholder="10001"
                    />
                    {errors.postalCode && (
                      <p className="text-sm text-red-600 mt-1">{errors.postalCode.message}</p>
                    )}
                  </div>
                </div>

                <div>
                  <Label htmlFor="country">Country *</Label>
                  <Input
                    id="country"
                    {...register('country', { required: 'Country is required' })}
                    placeholder="USA"
                  />
                  {errors.country && (
                    <p className="text-sm text-red-600 mt-1">{errors.country.message}</p>
                  )}
                </div>

                {/* Optional Fields */}
                <div className="border-t pt-4 mt-4">
                  <h3 className="font-semibold mb-3">Additional Information (Optional)</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <Label htmlFor="industry">Industry</Label>
                      <Input id="industry" {...register('industry')} placeholder="Furniture Retail" />
                    </div>

                    <div>
                      <Label htmlFor="website">Website</Label>
                      <Input
                        id="website"
                        type="url"
                        {...register('website')}
                        placeholder="https://company.com"
                      />
                    </div>

                    <div>
                      <Label htmlFor="employeeCount">Number of Employees</Label>
                      <Input
                        id="employeeCount"
                        type="number"
                        {...register('employeeCount', { valueAsNumber: true })}
                        placeholder="25"
                      />
                    </div>

                    <div>
                      <Label htmlFor="annualRevenue">Annual Revenue (USD)</Label>
                      <Input
                        id="annualRevenue"
                        type="number"
                        {...register('annualRevenue', { valueAsNumber: true })}
                        placeholder="2000000"
                      />
                    </div>
                  </div>
                </div>

                <div className="flex justify-end pt-4">
                  <Button type="button" onClick={onNext}>
                    Next: Admin User
                  </Button>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Admin User Tab */}
          <TabsContent value="admin">
            <Card>
              <CardHeader>
                <CardTitle>Admin User Account</CardTitle>
                <CardDescription>
                  Create the primary admin user for this partner company
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="adminUser.firstName">First Name *</Label>
                    <Input
                      id="adminUser.firstName"
                      {...register('adminUser.firstName', { required: 'First name is required' })}
                      placeholder="John"
                    />
                    {errors.adminUser?.firstName && (
                      <p className="text-sm text-red-600 mt-1">
                        {errors.adminUser.firstName.message}
                      </p>
                    )}
                  </div>

                  <div>
                    <Label htmlFor="adminUser.lastName">Last Name *</Label>
                    <Input
                      id="adminUser.lastName"
                      {...register('adminUser.lastName', { required: 'Last name is required' })}
                      placeholder="Doe"
                    />
                    {errors.adminUser?.lastName && (
                      <p className="text-sm text-red-600 mt-1">
                        {errors.adminUser.lastName.message}
                      </p>
                    )}
                  </div>
                </div>

                <div>
                  <Label htmlFor="adminUser.email">Email Address *</Label>
                  <Input
                    id="adminUser.email"
                    type="email"
                    {...register('adminUser.email', {
                      required: 'Email is required',
                      pattern: {
                        value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                        message: 'Invalid email address',
                      },
                    })}
                    placeholder="john@company.com"
                  />
                  {errors.adminUser?.email && (
                    <p className="text-sm text-red-600 mt-1">{errors.adminUser.email.message}</p>
                  )}
                </div>

                <div>
                  <Label htmlFor="adminUser.password">Temporary Password *</Label>
                  <Input
                    id="adminUser.password"
                    type="password"
                    {...register('adminUser.password', {
                      required: 'Password is required',
                      minLength: {
                        value: 8,
                        message: 'Password must be at least 8 characters',
                      },
                      pattern: {
                        value: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/,
                        message:
                          'Password must contain uppercase, lowercase, number, and special character',
                      },
                    })}
                    placeholder="••••••••"
                  />
                  {errors.adminUser?.password && (
                    <p className="text-sm text-red-600 mt-1">
                      {errors.adminUser.password.message}
                    </p>
                  )}
                  <p className="text-xs text-gray-500 mt-1">
                    The user will be prompted to change this password on first login
                  </p>
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <h4 className="font-semibold text-blue-900 mb-2">Account Status</h4>
                  <p className="text-sm text-blue-800">
                    The partner company will be created with <strong>Active</strong> status. The admin
                    user can login immediately.
                  </p>
                </div>

                <div className="flex justify-between pt-4">
                  <Button type="button" variant="outline" onClick={() => setActiveTab('company')}>
                    Back to Company Info
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Creating Partner...' : 'Create Partner'}
                  </Button>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </form>
    </div>
  );
}

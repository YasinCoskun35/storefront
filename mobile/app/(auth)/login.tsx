import React, { useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { Button } from '../../components/ui/Button';
import { Colors } from '../../constants/colors';
import { authApi } from '../../lib/api/auth';
import { useAuth } from '../../lib/auth';

type LoginMode = 'partner' | 'admin';

export default function LoginScreen() {
  const { t } = useTranslation();
  const { signIn } = useAuth();
  const [mode, setMode] = useState<LoginMode>('partner');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleLogin() {
    if (!email.trim() || !password.trim()) {
      Alert.alert('Validation', 'Please enter your email and password.');
      return;
    }

    setLoading(true);
    try {
      if (mode === 'admin') {
        const res = await authApi.adminLogin(email.trim(), password);
        const u = res.data.user;
        await signIn(res.data.accessToken, {
          id: u.id,
          email: u.email,
          firstName: u.firstName,
          lastName: u.lastName,
          role: u.roles[0] ?? 'Admin',
          isAdmin: true,
          company: { id: '', name: '', status: '' },
        });
      } else {
        const res = await authApi.login(email.trim(), password);
        await signIn(res.data.accessToken, { ...res.data.user, isAdmin: false });
      }
    } catch (err: any) {
      const message =
        err.response?.data?.message ??
        err.response?.data?.title ??
        'Login failed. Please check your credentials.';
      Alert.alert('Sign In Failed', message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView
          contentContainerStyle={styles.container}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.header}>
            <Text style={styles.logo}>Storefront</Text>
            <Text style={styles.subtitle}>
              {mode === 'admin' ? 'Admin Panel' : t('auth.partnerPortal')}
            </Text>
          </View>

          {/* Mode toggle */}
          <View style={styles.toggle}>
            <TouchableOpacity
              style={[styles.toggleBtn, mode === 'partner' && styles.toggleBtnActive]}
              onPress={() => setMode('partner')}
            >
              <Text style={[styles.toggleText, mode === 'partner' && styles.toggleTextActive]}>
                Partner
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.toggleBtn, mode === 'admin' && styles.toggleBtnActive]}
              onPress={() => setMode('admin')}
            >
              <Text style={[styles.toggleText, mode === 'admin' && styles.toggleTextActive]}>
                Admin
              </Text>
            </TouchableOpacity>
          </View>

          <View style={styles.form}>
            <Text style={styles.title}>{t('auth.signIn')}</Text>
            <Text style={styles.desc}>
              {mode === 'admin'
                ? 'Enter your admin credentials.'
                : t('auth.loginSubtitle')}
            </Text>

            <View style={styles.field}>
              <Text style={styles.label}>{t('auth.email')}</Text>
              <TextInput
                style={styles.input}
                value={email}
                onChangeText={setEmail}
                placeholder={mode === 'admin' ? 'admin@storefront.com' : 'partner@company.com'}
                placeholderTextColor={Colors.textMuted}
                keyboardType="email-address"
                autoCapitalize="none"
                autoCorrect={false}
                returnKeyType="next"
              />
            </View>

            <View style={styles.field}>
              <Text style={styles.label}>{t('auth.password')}</Text>
              <TextInput
                style={styles.input}
                value={password}
                onChangeText={setPassword}
                placeholder="••••••••"
                placeholderTextColor={Colors.textMuted}
                secureTextEntry
                returnKeyType="done"
                onSubmitEditing={handleLogin}
              />
            </View>

            <Button
              title={loading ? t('auth.signingIn') : t('auth.signIn')}
              onPress={handleLogin}
              loading={loading}
              size="lg"
              style={styles.submitButton}
            />

            {mode === 'partner' && (
              <Text style={styles.hint}>
                Don't have an account? Contact your administrator.
              </Text>
            )}
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  flex: { flex: 1 },
  container: { flexGrow: 1, justifyContent: 'center', padding: 24 },
  header: { alignItems: 'center', marginBottom: 24 },
  logo: { fontSize: 28, fontWeight: '800', color: Colors.primary, letterSpacing: -0.5 },
  subtitle: { fontSize: 14, color: Colors.textSecondary, marginTop: 4 },
  toggle: {
    flexDirection: 'row',
    backgroundColor: Colors.surface,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    marginBottom: 20,
    overflow: 'hidden',
  },
  toggleBtn: { flex: 1, paddingVertical: 10, alignItems: 'center' },
  toggleBtnActive: { backgroundColor: Colors.primary },
  toggleText: { fontSize: 14, fontWeight: '600', color: Colors.textSecondary },
  toggleTextActive: { color: '#fff' },
  form: {
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 24,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  title: { fontSize: 20, fontWeight: '700', color: Colors.text, marginBottom: 6 },
  desc: { fontSize: 14, color: Colors.textSecondary, marginBottom: 24 },
  field: { marginBottom: 16 },
  label: { fontSize: 13, fontWeight: '600', color: Colors.text, marginBottom: 6 },
  input: {
    borderWidth: 1.5,
    borderColor: Colors.border,
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 12,
    fontSize: 15,
    color: Colors.text,
    backgroundColor: Colors.background,
  },
  submitButton: { marginTop: 8 },
  hint: { marginTop: 16, fontSize: 12, color: Colors.textMuted, textAlign: 'center' },
});

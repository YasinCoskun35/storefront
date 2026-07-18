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

export default function LoginScreen() {
  const { t } = useTranslation();
  const { signIn } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  // Admin mode is hidden — tap the logo 5 times to reveal it (internal use)
  const [logoTaps, setLogoTaps] = useState(0);
  const [adminMode, setAdminMode] = useState(false);

  function handleLogoTap() {
    const next = logoTaps + 1;
    setLogoTaps(next);
    if (next >= 5) {
      setAdminMode((prev) => !prev);
      setLogoTaps(0);
    }
  }

  async function handleLogin() {
    if (!email.trim() || !password.trim()) {
      Alert.alert('Validation', 'Please enter your email and password.');
      return;
    }

    setLoading(true);
    try {
      if (adminMode) {
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
            <TouchableOpacity onPress={handleLogoTap} activeOpacity={1}>
              <Text style={styles.logo}>Storefront</Text>
            </TouchableOpacity>
            <Text style={styles.subtitle}>
              {adminMode ? 'Admin Panel' : t('auth.partnerPortal')}
            </Text>
          </View>

          <View style={styles.form}>
            <Text style={styles.title}>{t('auth.signIn')}</Text>
            <Text style={styles.desc}>
              {adminMode
                ? 'Enter your admin credentials.'
                : t('auth.loginSubtitle')}
            </Text>

            <View style={styles.field}>
              <Text style={styles.label}>{t('auth.email')}</Text>
              <TextInput
                style={styles.input}
                value={email}
                onChangeText={setEmail}
                placeholder={adminMode ? 'admin@storefront.com' : 'partner@company.com'}
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

            {!adminMode && (
              <Text style={styles.hint}>
                {t('auth.contactAdmin')}
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

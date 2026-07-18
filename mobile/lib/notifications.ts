import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import { Platform } from 'react-native';
import { api } from './api';

// Configure how notifications are presented when the app is in the foreground
Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge:  false,
    shouldShowBanner: true,
    shouldShowList: true,
  }),
});

/**
 * Requests notification permission and registers the Expo push token with the backend.
 * Should be called once after successful login.
 */
export async function registerPushToken(): Promise<void> {
  // Push notifications require a real device
  if (!Device.isDevice) {
    return;
  }

  const { status: existingStatus } = await Notifications.getPermissionsAsync();
  let finalStatus = existingStatus;

  if (existingStatus !== 'granted') {
    const { status } = await Notifications.requestPermissionsAsync();
    finalStatus = status;
  }

  if (finalStatus !== 'granted') {
    // User denied — silently skip; we won't pester them again this session
    return;
  }

  // On Android, a notification channel is required for foreground notifications
  if (Platform.OS === 'android') {
    await Notifications.setNotificationChannelAsync('orders', {
      name:       'Order Updates',
      importance: Notifications.AndroidImportance.HIGH,
      vibrationPattern: [0, 250, 250, 250],
      lightColor: '#2563EB',
    });
  }

  try {
    const tokenData = await Notifications.getExpoPushTokenAsync();
    const token = tokenData.data;

    // Register with backend — fire and forget, never block login
    await api.post('/api/identity/partners/push-token', { pushToken: token });
  } catch {
    // Non-fatal — app works fine without push notifications
  }
}

/**
 * Clears the push token from the backend on logout.
 */
export async function unregisterPushToken(): Promise<void> {
  try {
    await api.post('/api/identity/partners/push-token', { pushToken: null });
  } catch {
    // Non-fatal
  }
}

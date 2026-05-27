import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { AuthProvider } from '../src/context/AuthContext';
import { initDatabase } from '../src/database/db';
import { syncDataWithServer, initSyncListener } from '../src/services/syncService';

function AppInit() {
  useEffect(() => {
    initDatabase();
    syncDataWithServer();
    const unsub = initSyncListener();
    return () => {
      if (typeof unsub === 'function') unsub();
    };
  }, []);
  return null;
}

export default function RootLayout() {
  return (
    <AuthProvider>
      <AppInit />
      <StatusBar style="dark" />
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen name="login" options={{ headerShown: false }} />
        <Stack.Screen name="register" options={{ headerShown: false }} />
        <Stack.Screen name="search-results" options={{ title: 'Vuelos Disponibles', headerShown: true }} />
        <Stack.Screen name="payment" options={{ title: 'Pago', headerShown: true }} />
        <Stack.Screen name="+not-found" />
      </Stack>
    </AuthProvider>
  );
}

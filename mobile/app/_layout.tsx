import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { initDatabase } from '../src/database/db';
import { syncDataWithServer, initSyncListener } from '../src/services/syncService';

export default function RootLayout() {
  useEffect(() => {
    initDatabase();
    
    // Ejecutar sincronización inicial con el servidor central
    syncDataWithServer();
    
    // Escuchar cambios de conexión de red para sincronizar en segundo plano
    const unsubscribe = initSyncListener();
    return () => {
      if (typeof unsubscribe === 'function') unsubscribe();
    };
  }, []);

  return (
    <>
      <StatusBar style="dark" />
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen name="+not-found" />
      </Stack>
    </>
  );
}

import React, { useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, FlatList, RefreshControl,
  TouchableOpacity, Alert, ActivityIndicator,
} from 'react-native';
import { useFocusEffect, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { reservationService, type Reservation } from '../../src/services/reservationService';
import { useAuth } from '../../src/context/AuthContext';
import { theme } from '../../src/styles/theme';

const BADGE: Record<string, { bg: string; text: string; label: string }> = {
  pendiente: { bg: '#FFF3CD', text: '#856404', label: 'Pendiente' },
  pagado:    { bg: '#D1E7DD', text: '#0F5132', label: 'Pagado' },
  cancelado: { bg: '#F8D7DA', text: '#842029', label: 'Cancelado' },
};

export default function ReservationsScreen() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuth();
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(false);
  const [cancelling, setCancelling] = useState<string | null>(null);

  const fetchReservations = useCallback(() => {
    if (!user) return;
    setLoading(true);
    reservationService.getByUser(user.id)
      .then(setReservations)
      .catch(() => Alert.alert('Error', 'No se pudieron cargar tus reservaciones.'))
      .finally(() => setLoading(false));
  }, [user]);

  useFocusEffect(fetchReservations);

  const handleCancel = (cod: string) => {
    Alert.alert(
      'Cancelar reservación',
      `¿Seguro que deseas cancelar la reservación #${cod}?`,
      [
        { text: 'No', style: 'cancel' },
        {
          text: 'Sí, cancelar',
          style: 'destructive',
          onPress: async () => {
            setCancelling(cod);
            try {
              await reservationService.cancel(cod);
              fetchReservations();
            } catch {
              Alert.alert('Error', 'No se pudo cancelar la reservación.');
            } finally {
              setCancelling(null);
            }
          },
        },
      ]
    );
  };

  if (!isAuthenticated) {
    return (
      <SafeAreaView style={styles.safe} edges={['bottom']}>
        <View style={styles.center}>
          <Text style={styles.emptyText}>Inicia sesión para ver tus reservaciones.</Text>
          <TouchableOpacity style={styles.btn} onPress={() => router.push('/login')}>
            <Text style={styles.btnText}>Iniciar sesión</Text>
          </TouchableOpacity>
        </View>
      </SafeAreaView>
    );
  }

  const renderItem = ({ item }: { item: Reservation }) => {
    const badge = BADGE[item.estado_Pago] ?? BADGE.pendiente;
    return (
      <View style={styles.card}>
        <View style={styles.cardHeader}>
          <Text style={styles.reservCode}>#{item.cod_Reservacion}</Text>
          <View style={[styles.badge, { backgroundColor: badge.bg }]}>
            <Text style={[styles.badgeText, { color: badge.text }]}>{badge.label}</Text>
          </View>
        </View>
        <Text style={styles.flightText}>Vuelo {item.flightNumber}</Text>
        <Text style={styles.dateText}>{new Date(item.fecha).toLocaleDateString('es-CR')}</Text>

        {item.estado_Pago === 'pendiente' && (
          <View style={styles.actions}>
            <TouchableOpacity
              style={styles.payBtn}
              onPress={() => router.push(`/payment?cod=${item.cod_Reservacion}`)}
            >
              <Text style={styles.payBtnText}>Pagar</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={styles.cancelBtn}
              disabled={cancelling === item.cod_Reservacion}
              onPress={() => handleCancel(item.cod_Reservacion)}
            >
              {cancelling === item.cod_Reservacion
                ? <ActivityIndicator size="small" color="#842029" />
                : <Text style={styles.cancelBtnText}>Cancelar</Text>
              }
            </TouchableOpacity>
          </View>
        )}
      </View>
    );
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {loading && reservations.length === 0 ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={theme.colors.primary} />
        </View>
      ) : reservations.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyText}>No tienes reservaciones aún.</Text>
          <TouchableOpacity style={styles.btn} onPress={() => router.push('/(tabs)')}>
            <Text style={styles.btnText}>Buscar vuelos</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <FlatList
          data={reservations}
          keyExtractor={item => item.cod_Reservacion}
          renderItem={renderItem}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={fetchReservations} />}
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  emptyText: { fontSize: 16, color: theme.colors.textMuted, textAlign: 'center', marginBottom: 16 },
  btn: { backgroundColor: theme.colors.primary, paddingVertical: 12, paddingHorizontal: 28, borderRadius: 10 },
  btnText: { color: '#FFF', fontWeight: 'bold', fontSize: 15 },
  list: { padding: 16, paddingBottom: 40 },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 14,
    padding: 18,
    marginBottom: 14,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 6,
    elevation: 2,
  },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  reservCode: { fontSize: 17, fontWeight: 'bold', color: theme.colors.textDark },
  badge: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 12 },
  badgeText: { fontSize: 11, fontWeight: 'bold' },
  flightText: { fontSize: 14, color: theme.colors.textDark, marginBottom: 4 },
  dateText: { fontSize: 13, color: theme.colors.textMuted, marginBottom: 12 },
  actions: { flexDirection: 'row', gap: 10 },
  payBtn: {
    flex: 1, backgroundColor: theme.colors.primary,
    paddingVertical: 10, borderRadius: 8, alignItems: 'center',
  },
  payBtnText: { color: '#FFF', fontWeight: 'bold', fontSize: 14 },
  cancelBtn: {
    flex: 1, borderWidth: 1.5, borderColor: '#842029',
    paddingVertical: 10, borderRadius: 8, alignItems: 'center',
  },
  cancelBtnText: { color: '#842029', fontWeight: 'bold', fontSize: 14 },
});

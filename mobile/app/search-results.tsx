import React, { useState, useEffect } from 'react';
import {
  View, Text, StyleSheet, FlatList, TouchableOpacity,
  Alert, ActivityIndicator,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { flightService, type Flight } from '../src/services/flightService';
import { reservationService } from '../src/services/reservationService';
import { useAuth } from '../src/context/AuthContext';
import { theme } from '../src/styles/theme';

export default function SearchResultsScreen() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuth();
  const { originId, destinationId, originName, destinationName, fecha } = useLocalSearchParams<{
    originId: string;
    destinationId: string;
    originName: string;
    destinationName: string;
    fecha: string;
  }>();

  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(true);
  const [reserving, setReserving] = useState<string | null>(null);

  useEffect(() => {
    if (!originId || !destinationId) return;
    flightService.search(Number(originId), Number(destinationId), fecha || undefined)
      .then(setFlights)
      .catch(() => Alert.alert('Error', 'No se pudieron cargar los vuelos.'))
      .finally(() => setLoading(false));
  }, [originId, destinationId, fecha]);

  const handleReserve = async (flight: Flight) => {
    if (!isAuthenticated || !user) {
      Alert.alert(
        'Inicia sesión',
        'Necesitas una cuenta para reservar.',
        [
          { text: 'Cancelar', style: 'cancel' },
          { text: 'Iniciar sesión', onPress: () => router.push('/login') },
        ]
      );
      return;
    }

    Alert.alert(
      'Confirmar reservación',
      `Vuelo ${flight.flightNumber}\n${new Date(flight.hora_Salida).toLocaleString('es-CR')}\nPrecio: $${flight.precio}`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Reservar',
          onPress: async () => {
            setReserving(flight.flightNumber);
            try {
              const cod = await reservationService.create(user.id, flight.flightNumber);
              router.push(`/payment?cod=${cod}&monto=${flight.precio}`);
            } catch (err: any) {
              const msg = err.response?.data?.title || 'No se pudo completar la reservación.';
              Alert.alert('Error', msg);
            } finally {
              setReserving(null);
            }
          },
        },
      ]
    );
  };

  const renderFlight = ({ item }: { item: Flight }) => (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <Text style={styles.route}>
          {item.origin?.name ?? originName} → {item.destination?.name ?? destinationName}
        </Text>
        <Text style={styles.price}>${item.precio}</Text>
      </View>
      <View style={styles.times}>
        <View>
          <Text style={styles.timeLabel}>SALIDA</Text>
          <Text style={styles.timeValue}>{new Date(item.hora_Salida).toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}</Text>
          <Text style={styles.dateValue}>{new Date(item.hora_Salida).toLocaleDateString('es-CR')}</Text>
        </View>
        <Text style={styles.planeIcon}>✈</Text>
        <View style={{ alignItems: 'flex-end' }}>
          <Text style={styles.timeLabel}>LLEGADA</Text>
          <Text style={styles.timeValue}>{new Date(item.hora_Llegada).toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}</Text>
          <Text style={styles.dateValue}>{new Date(item.hora_Llegada).toLocaleDateString('es-CR')}</Text>
        </View>
      </View>
      <Text style={styles.flightNum}>Vuelo {item.flightNumber} · {item.estado}</Text>
      <TouchableOpacity
        style={[styles.btn, reserving === item.flightNumber && styles.btnDisabled]}
        onPress={() => handleReserve(item)}
        disabled={reserving === item.flightNumber}
      >
        {reserving === item.flightNumber
          ? <ActivityIndicator color="#FFF" />
          : <Text style={styles.btnText}>Reservar</Text>
        }
      </TouchableOpacity>
    </View>
  );

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <View style={styles.subHeader}>
        <Text style={styles.subHeaderText}>
          {originName} → {destinationName}
        </Text>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={theme.colors.primary} />
        </View>
      ) : flights.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyText}>No se encontraron vuelos para esta ruta.</Text>
          <TouchableOpacity onPress={() => router.back()}>
            <Text style={styles.link}>Volver a buscar</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <FlatList
          data={flights}
          keyExtractor={item => item.flightNumber}
          renderItem={renderFlight}
          contentContainerStyle={styles.list}
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  subHeader: { paddingHorizontal: 20, paddingVertical: 12, backgroundColor: theme.colors.white, borderBottomWidth: 1, borderBottomColor: theme.colors.border },
  subHeaderText: { fontSize: 15, color: theme.colors.textMuted, fontWeight: '600' },
  list: { padding: 16, paddingBottom: 40 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  emptyText: { fontSize: 16, color: theme.colors.textMuted, textAlign: 'center', marginBottom: 16 },
  link: { color: theme.colors.primary, fontWeight: 'bold', fontSize: 15 },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 14,
    padding: 18,
    marginBottom: 16,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 8,
    elevation: 3,
    borderWidth: 1,
    borderColor: '#F0F0F0',
  },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14 },
  route: { fontSize: 16, fontWeight: 'bold', color: theme.colors.textDark, flex: 1 },
  price: { fontSize: 20, fontWeight: 'bold', color: '#00C853' },
  times: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 },
  timeLabel: { fontSize: 10, fontWeight: '700', color: theme.colors.textMuted, letterSpacing: 1 },
  timeValue: { fontSize: 22, fontWeight: 'bold', color: theme.colors.textDark },
  dateValue: { fontSize: 12, color: theme.colors.textMuted },
  planeIcon: { fontSize: 22, color: theme.colors.textMuted },
  flightNum: { fontSize: 12, color: theme.colors.textMuted, marginBottom: 14 },
  btn: {
    backgroundColor: theme.colors.primary,
    paddingVertical: 12,
    borderRadius: 10,
    alignItems: 'center',
  },
  btnDisabled: { opacity: 0.6 },
  btnText: { color: '#FFF', fontWeight: 'bold', fontSize: 15 },
});

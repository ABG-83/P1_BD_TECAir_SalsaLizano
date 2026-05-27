import React, { useState, useEffect, useRef } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView,
  ActivityIndicator, Alert, Platform, Modal, FlatList,
} from 'react-native';
import DateTimePicker, { type DateTimePickerEvent } from '@react-native-community/datetimepicker';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import NetInfo from '@react-native-community/netinfo';
import { flightService, type Airport } from '../../src/services/flightService';
import { useAuth } from '../../src/context/AuthContext';
import { theme } from '../../src/styles/theme';

type PickerField = 'origin' | 'destination' | null;

export default function SearchFlightsScreen() {
  const router = useRouter();
  const { user, isAuthenticated, logout } = useAuth();

  const [airports, setAirports] = useState<Airport[]>([]);
  const [loadingAirports, setLoadingAirports] = useState(true);
  const [originId, setOriginId] = useState('');
  const [destinationId, setDestinationId] = useState('');
  const [fecha, setFecha] = useState<Date | null>(null);
  const [showDatePicker, setShowDatePicker] = useState(false);
  const [activePicker, setActivePicker] = useState<PickerField>(null);
  const wasConnected = useRef<boolean | null>(null);

  const loadAirports = (showSpinner = false) => {
    if (showSpinner) setLoadingAirports(true);
    flightService.getAirports()
      .then(data => { if (data.length > 0) setAirports(data); })
      .catch(() => { if (showSpinner) Alert.alert('Error', 'No se pudieron cargar los aeropuertos.'); })
      .finally(() => setLoadingAirports(false));
  };

  useEffect(() => {
    loadAirports(true);

    const unsub = NetInfo.addEventListener(state => {
      const connected = !!state.isConnected;
      if (connected && wasConnected.current === false) {
        loadAirports(false);
      }
      wasConnected.current = connected;
    });

    return () => unsub();
  }, []);

  const onDateChange = (_event: DateTimePickerEvent, selected?: Date) => {
    setShowDatePicker(Platform.OS === 'ios');
    if (selected) setFecha(selected);
  };

  const handleWebDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.value) {
      setFecha(new Date(e.target.value + 'T12:00:00')); // T12:00:00 previene desajustes de zona horaria local
    }
  };

  const fechaStr = fecha ? fecha.toISOString().split('T')[0] : '';
  const fechaDisplay = fecha
    ? fecha.toLocaleDateString('es-CR', { day: '2-digit', month: '2-digit', year: 'numeric' })
    : 'dd/mm/aaaa';

  const originLabel = originId
    ? (airports.find(a => String(a.airportId) === originId)?.name ?? '¿Desde dónde viajas?')
    : '¿Desde dónde viajas?';
  const destLabel = destinationId
    ? (airports.find(a => String(a.airportId) === destinationId)?.name ?? '¿A dónde vas?')
    : '¿A dónde vas?';

  const destAirports = airports.filter(a => String(a.airportId) !== originId);
  const pickerData = activePicker === 'origin' ? airports : destAirports;

  const handlePickerSelect = (airport: Airport) => {
    if (activePicker === 'origin') {
      setOriginId(String(airport.airportId));
      if (destinationId === String(airport.airportId)) setDestinationId('');
    } else {
      setDestinationId(String(airport.airportId));
    }
    setActivePicker(null);
  };

  const handleSearch = () => {
    if (!originId || !destinationId) {
      Alert.alert('Selecciona origen y destino', 'Elige ambos aeropuertos para continuar.');
      return;
    }
    const origin = airports.find(a => String(a.airportId) === originId);
    const destination = airports.find(a => String(a.airportId) === destinationId);
    router.push(
      `/search-results?originId=${originId}&destinationId=${destinationId}&originName=${encodeURIComponent(origin?.name ?? '')}&destinationName=${encodeURIComponent(destination?.name ?? '')}${fechaStr ? `&fecha=${fechaStr}` : ''}`
    );
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled">
        {/* Promo Banner */}
        <View style={styles.promoBanner}>
          <Text style={styles.promoTitle}>¡Ofertas Especiales!</Text>
          <Text style={styles.promoText}>
            Descuentos exclusivos para estudiantes.{' '}
            <Text style={styles.promoLink} onPress={() => router.push('/(tabs)/promotions')}>
              Ver promociones →
            </Text>
          </Text>
        </View>

        {/* Search Card */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>Encuentra tu próximo vuelo</Text>

          {loadingAirports ? (
            <ActivityIndicator size="small" color={theme.colors.primary} style={{ marginVertical: 20 }} />
          ) : (
            <>
              <Text style={styles.label}>ORIGEN</Text>
              <TouchableOpacity
                style={styles.selectBtn}
                onPress={() => setActivePicker('origin')}
                activeOpacity={0.7}
              >
                <Text style={[styles.selectText, !originId && styles.selectPlaceholder]}>
                  {originLabel}
                </Text>
                <Text style={styles.chevron}>▾</Text>
              </TouchableOpacity>

              <Text style={styles.label}>DESTINO</Text>
              <TouchableOpacity
                style={styles.selectBtn}
                onPress={() => setActivePicker('destination')}
                activeOpacity={0.7}
              >
                <Text style={[styles.selectText, !destinationId && styles.selectPlaceholder]}>
                  {destLabel}
                </Text>
                <Text style={styles.chevron}>▾</Text>
              </TouchableOpacity>

              <Text style={styles.label}>FECHA DE SALIDA</Text>
              {Platform.OS === 'web' ? (
                <input
                  type="date"
                  style={{
                    border: 'none',
                    borderBottom: `1px solid ${theme.colors.border}`,
                    fontSize: 16,
                    paddingTop: 10,
                    paddingBottom: 10,
                    marginBottom: 20,
                    color: theme.colors.textDark,
                    outline: 'none',
                    width: '100%',
                    backgroundColor: 'transparent',
                    fontFamily: 'inherit',
                  }}
                  min={new Date().toISOString().split('T')[0]}
                  value={fechaStr}
                  onChange={handleWebDateChange}
                />
              ) : (
                <>
                  <TouchableOpacity
                    style={styles.selectBtn}
                    onPress={() => setShowDatePicker(true)}
                    activeOpacity={0.7}
                  >
                    <Text style={[styles.selectText, !fecha && styles.selectPlaceholder]}>
                      {fechaDisplay}
                    </Text>
                    <Text style={styles.chevron}>📅</Text>
                  </TouchableOpacity>

                  {showDatePicker && (
                    <DateTimePicker
                      value={fecha ?? new Date()}
                      mode="date"
                      display={Platform.OS === 'ios' ? 'spinner' : 'default'}
                      minimumDate={new Date()}
                      onChange={onDateChange}
                    />
                  )}
                </>
              )}

              <TouchableOpacity style={styles.searchBtn} onPress={handleSearch}>
                <Text style={styles.searchBtnText}>Buscar Vuelos</Text>
              </TouchableOpacity>
            </>
          )}
        </View>

        {/* User Session */}
        <View style={styles.sessionCard}>
          {isAuthenticated && user ? (
            <>
              <Text style={styles.sessionGreeting}>Hola, <Text style={{ fontWeight: 'bold' }}>{user.nombre}</Text></Text>
              {user.millas > 0 && <Text style={styles.sessionMillas}>Millas acumuladas: {user.millas}</Text>}
              <TouchableOpacity onPress={logout} style={styles.logoutBtn}>
                <Text style={styles.logoutText}>Cerrar sesión</Text>
              </TouchableOpacity>
            </>
          ) : (
            <>
              <Text style={styles.sessionPrompt}>Inicia sesión para reservar y ver tus vuelos</Text>
              <View style={styles.authRow}>
                <TouchableOpacity style={styles.loginBtn} onPress={() => router.push('/login')}>
                  <Text style={styles.loginText}>Iniciar sesión</Text>
                </TouchableOpacity>
                <TouchableOpacity style={styles.registerBtn} onPress={() => router.push('/register')}>
                  <Text style={styles.registerText}>Registrarse</Text>
                </TouchableOpacity>
              </View>
            </>
          )}
        </View>
      </ScrollView>

      {/* Airport Picker Modal */}
      <Modal
        visible={activePicker !== null}
        animationType="slide"
        transparent
        onRequestClose={() => setActivePicker(null)}
      >
        <TouchableOpacity
          style={styles.modalOverlay}
          activeOpacity={1}
          onPress={() => setActivePicker(null)}
        >
          <View style={styles.modalSheet}>
            <View style={styles.modalHandle} />
            <Text style={styles.modalTitle}>
              {activePicker === 'origin' ? '¿Desde dónde viajas?' : '¿A dónde vas?'}
            </Text>
            <FlatList
              data={pickerData}
              keyExtractor={item => String(item.airportId)}
              renderItem={({ item }) => {
                const isSelected = activePicker === 'origin'
                  ? String(item.airportId) === originId
                  : String(item.airportId) === destinationId;
                return (
                  <TouchableOpacity
                    style={[styles.airportItem, isSelected && styles.airportItemSelected]}
                    onPress={() => handlePickerSelect(item)}
                    activeOpacity={0.7}
                  >
                    <Text style={[styles.airportItemText, isSelected && styles.airportItemTextSelected]}>
                      {item.name}
                    </Text>
                    {isSelected && <Text style={styles.checkMark}>✓</Text>}
                  </TouchableOpacity>
                );
              }}
              ItemSeparatorComponent={() => <View style={styles.separator} />}
              showsVerticalScrollIndicator={false}
            />
          </View>
        </TouchableOpacity>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  container: { padding: 16, paddingBottom: 40 },
  promoBanner: {
    backgroundColor: '#FDF4F5',
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    borderLeftWidth: 4,
    borderLeftColor: theme.colors.accent,
  },
  promoTitle: { fontSize: 14, fontWeight: 'bold', color: theme.colors.accent, marginBottom: 4 },
  promoText: { fontSize: 13, color: theme.colors.textMuted },
  promoLink: { color: theme.colors.textDark, fontWeight: 'bold' },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    padding: 20,
    marginBottom: 16,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 8,
    elevation: 3,
  },
  cardTitle: { fontSize: 18, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 20 },
  label: { fontSize: 11, fontWeight: '700', color: theme.colors.textMuted, marginBottom: 6, letterSpacing: 1 },
  selectBtn: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    paddingVertical: 12,
    marginBottom: 20,
    minHeight: 44,
  },
  selectText: { fontSize: 16, color: theme.colors.textDark, flex: 1 },
  selectPlaceholder: { color: theme.colors.textMuted },
  chevron: { fontSize: 16, color: theme.colors.textMuted, marginLeft: 8 },
  searchBtn: {
    backgroundColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 4,
  },
  searchBtnText: { color: '#FFF', fontWeight: 'bold', fontSize: 16 },
  sessionCard: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    padding: 20,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 8,
    elevation: 3,
  },
  sessionGreeting: { fontSize: 15, color: theme.colors.textDark, marginBottom: 4 },
  sessionMillas: { fontSize: 13, color: theme.colors.textMuted, marginBottom: 12 },
  logoutBtn: { alignSelf: 'flex-start' },
  logoutText: { color: theme.colors.textMuted, fontSize: 14, fontWeight: '600' },
  sessionPrompt: { fontSize: 14, color: theme.colors.textMuted, marginBottom: 14 },
  authRow: { flexDirection: 'row', gap: 12 },
  loginBtn: {
    flex: 1, backgroundColor: theme.colors.primary,
    paddingVertical: 12, borderRadius: 10, alignItems: 'center',
  },
  loginText: { color: '#FFF', fontWeight: 'bold' },
  registerBtn: {
    flex: 1, borderWidth: 1.5, borderColor: theme.colors.primary,
    paddingVertical: 12, borderRadius: 10, alignItems: 'center',
  },
  registerText: { color: theme.colors.primary, fontWeight: 'bold' },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.45)',
    justifyContent: 'flex-end',
  },
  modalSheet: {
    backgroundColor: theme.colors.white,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 20,
    maxHeight: '75%',
  },
  modalHandle: {
    width: 40,
    height: 4,
    backgroundColor: '#DDD',
    borderRadius: 2,
    alignSelf: 'center',
    marginBottom: 16,
  },
  modalTitle: {
    fontSize: 17,
    fontWeight: 'bold',
    color: theme.colors.textDark,
    marginBottom: 16,
    textAlign: 'center',
  },
  airportItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 14,
    paddingHorizontal: 4,
  },
  airportItemSelected: {
    backgroundColor: '#F0F4FF',
    borderRadius: 8,
    paddingHorizontal: 10,
  },
  airportItemText: { fontSize: 16, color: theme.colors.textDark, flex: 1 },
  airportItemTextSelected: { color: theme.colors.primary, fontWeight: '600' },
  checkMark: { fontSize: 16, color: theme.colors.primary, marginLeft: 8 },
  separator: { height: 1, backgroundColor: theme.colors.border },
});

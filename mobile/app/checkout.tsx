import React, { useState, useEffect } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  TextInput, 
  TouchableOpacity, 
  ScrollView, 
  Switch, 
  Alert,
  KeyboardAvoidingView,
  Platform
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { theme } from '../src/styles/theme';
import db, { addOfflineReservation } from '../src/database/db';
import { SafeAreaView } from 'react-native-safe-area-context';

export default function CheckoutScreen() {
  const router = useRouter();
  const { vueloId } = useLocalSearchParams();
  
  const [vuelo, setVuelo] = useState<any>(null);

  // Formulario Usuario
  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');
  const [correo, setCorreo] = useState('');
  const [esEstudiante, setEsEstudiante] = useState(false);
  const [universidad, setUniversidad] = useState('');
  const [carnet, setCarnet] = useState('');

  // Maletas
  const [maletas, setMaletas] = useState(0);

  // Pago (Simulado)
  const [tarjeta, setTarjeta] = useState('');
  const [vencimiento, setVencimiento] = useState('');
  const [cvv, setCvv] = useState('');

  useEffect(() => {
    // Al cargar la pantalla, recuperar datos del vuelo de forma offline
    if (Platform.OS !== 'web' && db && vueloId) {
      const v = db.getFirstSync("SELECT * FROM Vuelos WHERE id = ?", [Number(vueloId)]);
      if (v) setVuelo(v);
    } else if (Platform.OS === 'web') {
      // Mock para debug web
      setVuelo({ id: Number(vueloId), origen: 'SJO', destino: 'MIA', precio: 250 });
    }
  }, [vueloId]);

  // Regla de maletas: 1ra $0, 2da $50, 3ra+ $75 c/u
  const getCostoMaletas = (cantidad: number) => {
    if (cantidad === 0 || cantidad === 1) return 0;
    if (cantidad === 2) return 50;
    return 50 + (cantidad - 2) * 75;
  };

  const costoBase = vuelo ? vuelo.precio : 0;
  const costoMaletas = getCostoMaletas(maletas);
  const costoTotal = costoBase + costoMaletas;

  // Modificar cantidad de maletas de forma segura
  const updateMaletas = (increment: number) => {
    setMaletas((prev) => Math.max(0, prev + increment));
  };

  const handleCheckout = () => {
    if (!nombre || !correo || !tarjeta || !vencimiento || !cvv) {
      Alert.alert("Campos incompletos", "Por favor completa toda la información requerida.");
      return;
    }
    if (esEstudiante && (!universidad || !carnet)) {
      Alert.alert("Campos de Estudiante incompletos", "Debes ingresar Universidad y Carné.");
      return;
    }

    // Flujo offline real:
    // 1. Aquí idealmente crearíamos un usuario en la tabla Usuarios primero de forma local y obtenemos su ID.
    // Usaremos un ID de usuario dummy para sincronización por esta fase.
    const usuarioMockId = 1; 
    
    // 2. Crear Reservación usando expo-crypto (via addOfflineReservation)
    const newReservationUuid = addOfflineReservation(Number(vueloId), usuarioMockId, "AsientoPendiente", maletas);

    // 3. Mostrar confirmación offline-friendly
    Alert.alert(
      "Reserva Exitosa (Modo Local)",
      "El pago ha sido procesado de manera local. La información se sincronizará automáticamente apenas detectemos red WiFi o Datos.",
      [
        { text: "Entendido", onPress: () => router.push('/reservations') }
      ]
    );
  };

  if (!vuelo) return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.loadingText}>Cargando información del vuelo...</Text>
    </SafeAreaView>
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView 
        style={styles.keyboardAvoid} 
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView contentContainerStyle={styles.scrollContent}>
          {/* Header del vuelo */}
          <View style={styles.summaryCard}>
            <Text style={styles.routeHeader}>{vuelo.origen} ✈️ {vuelo.destino}</Text>
            <Text style={styles.priceHeader}>Precio Base: ${costoBase}</Text>
          </View>

          {/* Formulario Pasajero */}
          <Text style={styles.sectionTitle}>Datos del Pasajero</Text>
          <View style={styles.card}>
            <TextInput style={styles.input} placeholder="Nombre Completo" value={nombre} onChangeText={setNombre} />
            <TextInput style={styles.input} placeholder="Teléfono" keyboardType="phone-pad" value={telefono} onChangeText={setTelefono} />
            <TextInput style={styles.input} placeholder="Correo Electrónico" keyboardType="email-address" value={correo} onChangeText={setCorreo} />
            
            <View style={styles.switchRow}>
              <Text style={styles.switchLabel}>¿Eres estudiante universitario?</Text>
              <Switch value={esEstudiante} onValueChange={setEsEstudiante} />
            </View>

            {esEstudiante && (
              <View style={styles.studentFields}>
                <TextInput style={styles.input} placeholder="Universidad" value={universidad} onChangeText={setUniversidad} />
                <TextInput style={styles.input} placeholder="Carné Universitario" value={carnet} onChangeText={setCarnet} />
              </View>
            )}
          </View>

          {/* Gestión de Maletas */}
          <Text style={styles.sectionTitle}>Equipaje Chequeado</Text>
          <View style={styles.card}>
            <View style={styles.luggageRow}>
              <View>
                <Text style={styles.luggageTitle}>Cantidad de Maletas</Text>
                <Text style={styles.luggageSubtitle}>1ra Gratis | 2da $50 | 3ra+ $75 c/u</Text>
              </View>
              <View style={styles.counter}>
                <TouchableOpacity style={styles.circleBtn} onPress={() => updateMaletas(-1)}>
                  <Text style={styles.circleBtnText}>-</Text>
                </TouchableOpacity>
                <Text style={styles.counterValue}>{maletas}</Text>
                <TouchableOpacity style={styles.circleBtn} onPress={() => updateMaletas(1)}>
                  <Text style={styles.circleBtnText}>+</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>

          {/* Simulación Pago */}
          <Text style={styles.sectionTitle}>Método de Pago</Text>
          <View style={styles.card}>
            <TextInput style={styles.input} placeholder="Número de Tarjeta" keyboardType="numeric" maxLength={16} value={tarjeta} onChangeText={setTarjeta} />
            <View style={styles.row}>
              <TextInput style={[styles.input, styles.halfInput]} placeholder="MM/YY" maxLength={5} value={vencimiento} onChangeText={setVencimiento} />
              <TextInput style={[styles.input, styles.halfInput]} placeholder="CVV" keyboardType="numeric" maxLength={4} secureTextEntry value={cvv} onChangeText={setCvv} />
            </View>
          </View>

          {/* Resumen Final */}
          <View style={styles.totalSection}>
            <View style={styles.totalRow}>
              <Text style={styles.totalLabel}>Vuelo Base</Text>
              <Text style={styles.totalValue}>${costoBase}</Text>
            </View>
            <View style={styles.totalRow}>
              <Text style={styles.totalLabel}>Costos de Equipaje</Text>
              <Text style={styles.totalValue}>${costoMaletas}</Text>
            </View>
            <View style={[styles.totalRow, styles.grandTotalRow]}>
              <Text style={styles.grandTotalLabel}>TOTAL A PAGAR</Text>
              <Text style={styles.grandTotalValue}>${costoTotal}</Text>
            </View>

            <TouchableOpacity style={styles.payButton} onPress={handleCheckout}>
              <Text style={styles.payButtonText}>Pagar y Reservar Offline</Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: { flex: 1, backgroundColor: '#FAFAFA' },
  keyboardAvoid: { flex: 1 },
  scrollContent: { padding: 20, paddingBottom: 50 },
  container: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  loadingText: { color: theme.colors.textMuted },
  
  // Headers
  summaryCard: {
    backgroundColor: theme.colors.primary,
    padding: 20,
    borderRadius: 16,
    marginBottom: 20,
  },
  routeHeader: { color: '#FFF', fontSize: 24, fontWeight: 'bold' },
  priceHeader: { color: '#E0F7FA', fontSize: 16, marginTop: 5 },
  
  sectionTitle: { fontSize: 16, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 10, marginLeft: 5 },
  
  // Cards & Inputs
  card: {
    backgroundColor: '#FFF',
    padding: 18,
    borderRadius: 12,
    marginBottom: 20,
    shadowColor: '#000',
    shadowOpacity: 0.04,
    shadowRadius: 5,
    elevation: 2,
    borderWidth: 1,
    borderColor: '#F0F0F0',
  },
  input: {
    borderBottomWidth: 1,
    borderBottomColor: '#E0E0E0',
    paddingVertical: 10,
    marginBottom: 15,
    fontSize: 16,
  },
  row: { flexDirection: 'row', justifyContent: 'space-between' },
  halfInput: { width: '48%' },
  
  // Switches
  switchRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginVertical: 10 },
  switchLabel: { fontSize: 15, color: theme.colors.text },
  studentFields: { marginTop: 10 },
  
  // Luggage 
  luggageRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  luggageTitle: { fontSize: 16, fontWeight: 'bold', color: theme.colors.text },
  luggageSubtitle: { fontSize: 12, color: theme.colors.textMuted, marginTop: 4 },
  counter: { flexDirection: 'row', alignItems: 'center' },
  circleBtn: {
    backgroundColor: '#F0F0F0',
    width: 36, height: 36,
    borderRadius: 18,
    alignItems: 'center', justifyContent: 'center'
  },
  circleBtnText: { fontSize: 20, fontWeight: 'bold', color: theme.colors.primary },
  counterValue: { marginHorizontal: 15, fontSize: 18, fontWeight: 'bold' },
  
  // Totals
  totalSection: { marginTop: 10 },
  totalRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  totalLabel: { fontSize: 16, color: theme.colors.textMuted },
  totalValue: { fontSize: 16, fontWeight: '600' },
  grandTotalRow: { marginTop: 10, paddingTop: 10, borderTopWidth: 1, borderTopColor: '#E0E0E0' },
  grandTotalLabel: { fontSize: 18, fontWeight: 'bold', color: theme.colors.textDark },
  grandTotalValue: { fontSize: 22, fontWeight: 'bold', color: '#00C853' },
  
  payButton: {
    backgroundColor: '#00C853',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginTop: 25,
  },
  payButtonText: { color: '#FFF', fontSize: 18, fontWeight: 'bold' }
});
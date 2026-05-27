import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet,
  ScrollView, Alert, ActivityIndicator, KeyboardAvoidingView, Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { checkInService, type BoardingPass } from '../../src/services/checkInService';
import { theme } from '../../src/styles/theme';

export default function CheckInScreen() {
  const [codReservacion, setCodReservacion] = useState('');
  const [apellidos, setApellidos] = useState('');
  const [loading, setLoading] = useState(false);
  const [boardingPass, setBoardingPass] = useState<BoardingPass | null>(null);

  const handleSubmit = async () => {
    if (!codReservacion.trim() || !apellidos.trim()) {
      Alert.alert('Campos requeridos', 'Ingresa el código de reservación y tus apellidos.');
      return;
    }
    setLoading(true);
    try {
      const pass = await checkInService.doCheckIn(codReservacion.trim(), apellidos.trim());
      setBoardingPass(pass);
    } catch (err: any) {
      const msg =
        err.response?.data?.message ||
        err.response?.data?.title ||
        'No se pudo realizar el check-in. Verifica los datos.';
      Alert.alert('Error', msg);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setBoardingPass(null);
    setCodReservacion('');
    setApellidos('');
  };

  if (boardingPass) {
    return (
      <SafeAreaView style={styles.safe} edges={['bottom']}>
        <ScrollView contentContainerStyle={styles.container}>
          <View style={styles.successBanner}>
            <Text style={styles.successIcon}>✓</Text>
            <Text style={styles.successTitle}>¡Check-in realizado con éxito!</Text>
          </View>

          <View style={styles.passCard}>
            <Text style={styles.passTitle}>Pase de Abordar</Text>

            <View style={styles.passRow}>
              <Text style={styles.passLabel}>Vuelo</Text>
              <Text style={styles.passValue}>#{boardingPass.num_Vuelo}</Text>
            </View>
            <View style={styles.divider} />

            <View style={styles.passRow}>
              <Text style={styles.passLabel}>Reservación</Text>
              <Text style={styles.passValue}>#{boardingPass.cod_Reservacion}</Text>
            </View>
            <View style={styles.divider} />

            <View style={styles.passRow}>
              <Text style={styles.passLabel}>Asiento</Text>
              <Text style={styles.passValueLarge}>{boardingPass.asiento}</Text>
            </View>
            <View style={styles.divider} />

            <View style={styles.passRow}>
              <Text style={styles.passLabel}>Puerta</Text>
              <Text style={styles.passValueLarge}>{boardingPass.puerta_Abordaje}</Text>
            </View>
            <View style={styles.divider} />

            <View style={styles.passRow}>
              <Text style={styles.passLabel}>Check-in</Text>
              <Text style={styles.passValue}>
                {new Date(boardingPass.hora_Impresion).toLocaleString('es-CR')}
              </Text>
            </View>
          </View>

          <TouchableOpacity style={styles.resetBtn} onPress={handleReset}>
            <Text style={styles.resetBtnText}>Nuevo Check-in</Text>
          </TouchableOpacity>
        </ScrollView>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled">
          <View style={styles.card}>
            <Text style={styles.cardTitle}>Pre-Chequeo</Text>
            <Text style={styles.cardSubtitle}>Ingresa tus datos para obtener tu pase de abordar.</Text>

            <Text style={styles.label}>CÓDIGO DE RESERVACIÓN</Text>
            <TextInput
              style={styles.input}
              placeholder="Ej: RES-001"
              placeholderTextColor={theme.colors.textMuted}
              autoCapitalize="characters"
              value={codReservacion}
              onChangeText={setCodReservacion}
            />

            <Text style={styles.label}>APELLIDOS</Text>
            <TextInput
              style={styles.input}
              placeholder="Tal como aparecen en la reserva"
              placeholderTextColor={theme.colors.textMuted}
              autoCapitalize="words"
              value={apellidos}
              onChangeText={setApellidos}
            />

            <TouchableOpacity style={styles.btn} onPress={handleSubmit} disabled={loading}>
              {loading
                ? <ActivityIndicator color="#FFF" />
                : <Text style={styles.btnText}>Realizar Check-In</Text>
              }
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  container: { padding: 20, paddingBottom: 48 },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    padding: 24,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 10,
    elevation: 4,
  },
  cardTitle: { fontSize: 22, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 6, textAlign: 'center' },
  cardSubtitle: { fontSize: 14, color: theme.colors.textMuted, textAlign: 'center', marginBottom: 28 },
  label: { fontSize: 11, fontWeight: '700', color: theme.colors.textMuted, marginBottom: 6, letterSpacing: 1 },
  input: {
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    fontSize: 16,
    paddingVertical: 10,
    marginBottom: 22,
    color: theme.colors.textDark,
  },
  btn: {
    backgroundColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 8,
  },
  btnText: { color: '#FFF', fontSize: 16, fontWeight: 'bold' },
  successBanner: {
    backgroundColor: '#D1FAE5',
    borderRadius: 14,
    padding: 20,
    alignItems: 'center',
    marginBottom: 20,
  },
  successIcon: { fontSize: 40, color: '#065F46', marginBottom: 8 },
  successTitle: { fontSize: 17, fontWeight: 'bold', color: '#065F46', textAlign: 'center' },
  passCard: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    padding: 24,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 10,
    elevation: 4,
    marginBottom: 20,
  },
  passTitle: { fontSize: 18, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 20, textAlign: 'center' },
  passRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingVertical: 12 },
  passLabel: { fontSize: 14, color: theme.colors.textMuted },
  passValue: { fontSize: 15, fontWeight: 'bold', color: theme.colors.textDark },
  passValueLarge: { fontSize: 24, fontWeight: 'bold', color: theme.colors.primary },
  divider: { height: 1, backgroundColor: theme.colors.border },
  resetBtn: {
    borderWidth: 1.5,
    borderColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: 10,
    alignItems: 'center',
  },
  resetBtnText: { color: theme.colors.primary, fontWeight: 'bold', fontSize: 15 },
});

import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet,
  ScrollView, Alert, ActivityIndicator, KeyboardAvoidingView, Platform,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { paymentService } from '../src/services/paymentService';
import { theme } from '../src/styles/theme';

export default function PaymentScreen() {
  const router = useRouter();
  const { cod, monto } = useLocalSearchParams<{ cod: string; monto: string }>();

  const [cardNumber, setCardNumber] = useState('');
  const [cardholderName, setCardholderName] = useState('');
  const [expirationDate, setExpirationDate] = useState('');
  const [cvv, setCvv] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const formatCard = (val: string) =>
    val.replace(/\D/g, '').replace(/(.{4})/g, '$1 ').trim().slice(0, 19);

  const formatExpiry = (val: string) => {
    const digits = val.replace(/\D/g, '');
    if (digits.length >= 3) return digits.slice(0, 2) + '/' + digits.slice(2, 4);
    return digits;
  };

  const handlePay = async () => {
    if (!cardNumber || !cardholderName || !expirationDate || !cvv) {
      Alert.alert('Campos requeridos', 'Completa todos los datos de la tarjeta.');
      return;
    }
    setLoading(true);
    try {
      await paymentService.process({
        reservationCode: cod,
        amount: Number(monto ?? 0),
        cardNumber: cardNumber.replace(/\s/g, ''),
        cardholderName,
        expirationDate,
        cvv,
      });
      setSuccess(true);
      setTimeout(() => router.replace('/(tabs)/reservations'), 2500);
    } catch (err: any) {
      const data = err.response?.data;
      let msg = 'No se pudo procesar el pago. Verifica los datos.';
      if (data?.errors) {
        const first = Object.values(data.errors as Record<string, string[]>)[0];
        if (first?.length) msg = first[0];
      } else if (data) {
        msg = data.message || data.Message || data.title || msg;
      }
      Alert.alert('Error de pago', msg);
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <SafeAreaView style={styles.safe}>
        <View style={styles.successContainer}>
          <Text style={styles.checkmark}>✓</Text>
          <Text style={styles.successTitle}>¡Pago exitoso!</Text>
          <Text style={styles.successSub}>
            Tu reservación <Text style={{ fontWeight: 'bold' }}>#{cod}</Text> ha sido confirmada.
          </Text>
          <Text style={styles.redirectText}>Redirigiendo a tus reservaciones...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled">
          <View style={styles.reservationBanner}>
            <Text style={styles.reservationLabel}>Reservación</Text>
            <Text style={styles.reservationCode}>#{cod}</Text>
            {monto ? <Text style={styles.amount}>Total a pagar: <Text style={styles.amountBold}>${Number(monto).toFixed(2)}</Text></Text> : null}
          </View>

          <View style={styles.card}>
            <Text style={styles.sectionTitle}>Datos de Tarjeta</Text>

            <Text style={styles.label}>NÚMERO DE TARJETA</Text>
            <TextInput
              style={styles.input}
              placeholder="1234 5678 9012 3456"
              placeholderTextColor={theme.colors.textMuted}
              keyboardType="numeric"
              maxLength={19}
              value={cardNumber}
              onChangeText={v => setCardNumber(formatCard(v))}
            />

            <Text style={styles.label}>NOMBRE DEL TITULAR</Text>
            <TextInput
              style={styles.input}
              placeholder="Como aparece en la tarjeta"
              placeholderTextColor={theme.colors.textMuted}
              autoCapitalize="characters"
              value={cardholderName}
              onChangeText={setCardholderName}
            />

            <View style={styles.row}>
              <View style={styles.half}>
                <Text style={styles.label}>VENCIMIENTO (MM/AA)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="MM/AA"
                  placeholderTextColor={theme.colors.textMuted}
                  maxLength={5}
                  value={expirationDate}
                  onChangeText={v => setExpirationDate(formatExpiry(v))}
                />
              </View>
              <View style={[styles.half, { marginLeft: 16 }]}>
                <Text style={styles.label}>CVV</Text>
                <TextInput
                  style={styles.input}
                  placeholder="123"
                  placeholderTextColor={theme.colors.textMuted}
                  keyboardType="numeric"
                  secureTextEntry
                  maxLength={4}
                  value={cvv}
                  onChangeText={v => setCvv(v.replace(/\D/g, ''))}
                />
              </View>
            </View>
          </View>

          <TouchableOpacity style={styles.btn} onPress={handlePay} disabled={loading}>
            {loading
              ? <ActivityIndicator color="#FFF" />
              : <Text style={styles.btnText}>Pagar ${Number(monto ?? 0).toFixed(2)}</Text>
            }
          </TouchableOpacity>

          <TouchableOpacity style={styles.laterBtn} onPress={() => router.replace('/(tabs)/reservations')}>
            <Text style={styles.laterText}>Pagar después</Text>
          </TouchableOpacity>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  container: { padding: 20, paddingBottom: 48 },
  reservationBanner: {
    backgroundColor: theme.colors.primary,
    borderRadius: 14,
    padding: 20,
    marginBottom: 20,
  },
  reservationLabel: { color: 'rgba(255,255,255,0.7)', fontSize: 12, fontWeight: '600', letterSpacing: 1 },
  reservationCode: { color: '#FFF', fontSize: 24, fontWeight: 'bold', marginTop: 4 },
  amount: { color: 'rgba(255,255,255,0.85)', fontSize: 14, marginTop: 6 },
  amountBold: { fontWeight: 'bold', color: '#FFF' },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 14,
    padding: 20,
    marginBottom: 20,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 8,
    elevation: 3,
  },
  sectionTitle: { fontSize: 15, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 16 },
  label: { fontSize: 11, fontWeight: '700', color: theme.colors.textMuted, marginBottom: 6, letterSpacing: 1 },
  input: {
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    fontSize: 16,
    paddingVertical: 10,
    marginBottom: 20,
    color: theme.colors.textDark,
  },
  row: { flexDirection: 'row' },
  half: { flex: 1 },
  btn: {
    backgroundColor: '#00C853',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginBottom: 12,
  },
  btnText: { color: '#FFF', fontSize: 17, fontWeight: 'bold' },
  laterBtn: { alignItems: 'center', paddingVertical: 8 },
  laterText: { color: theme.colors.textMuted, fontSize: 14 },
  successContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 32 },
  checkmark: { fontSize: 64, color: '#00C853', marginBottom: 16 },
  successTitle: { fontSize: 28, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 8 },
  successSub: { fontSize: 16, color: theme.colors.textMuted, textAlign: 'center', marginBottom: 8 },
  redirectText: { fontSize: 13, color: theme.colors.textMuted },
});

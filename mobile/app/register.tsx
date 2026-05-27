import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet,
  KeyboardAvoidingView, Platform, ScrollView, Alert, ActivityIndicator, Switch,
} from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { userService } from '../src/services/userService';
import { theme } from '../src/styles/theme';

export default function RegisterScreen() {
  const router = useRouter();

  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');
  const [correo, setCorreo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [esEstudiante, setEsEstudiante] = useState(false);
  const [universidad, setUniversidad] = useState('');
  const [carnet, setCarnet] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRegister = async () => {
    if (!nombre || !telefono || !correo || !contrasena) {
      Alert.alert('Campos requeridos', 'Completa todos los campos obligatorios.');
      return;
    }
    if (esEstudiante && (!universidad || !carnet)) {
      Alert.alert('Datos de estudiante', 'Ingresa tu universidad y carné.');
      return;
    }
    setLoading(true);
    try {
      await userService.create({
        nombre,
        telefono,
        correo,
        contrasena,
        ...(esEstudiante && { universidad, carnet }),
      });
      Alert.alert('¡Registro exitoso!', 'Ya puedes iniciar sesión.', [
        { text: 'Ingresar', onPress: () => router.replace('/login') },
      ]);
    } catch {
      Alert.alert('Error', 'No se pudo registrar. Verifica que el correo no esté en uso.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled">
          <View style={styles.header}>
            <Text style={styles.logo}>TECAir</Text>
            <Text style={styles.subtitle}>Crear una cuenta</Text>
          </View>

          <View style={styles.card}>
            <Text style={styles.label}>NOMBRE COMPLETO</Text>
            <TextInput style={styles.input} placeholder="Tu nombre y apellidos" placeholderTextColor={theme.colors.textMuted} value={nombre} onChangeText={setNombre} />

            <Text style={styles.label}>TELÉFONO</Text>
            <TextInput style={styles.input} placeholder="+506 8888 8888" placeholderTextColor={theme.colors.textMuted} keyboardType="phone-pad" value={telefono} onChangeText={setTelefono} />

            <Text style={styles.label}>CORREO ELECTRÓNICO</Text>
            <TextInput style={styles.input} placeholder="ejemplo@correo.com" placeholderTextColor={theme.colors.textMuted} keyboardType="email-address" autoCapitalize="none" value={correo} onChangeText={setCorreo} />

            <Text style={styles.label}>CONTRASEÑA</Text>
            <TextInput style={styles.input} placeholder="Crea una contraseña segura" placeholderTextColor={theme.colors.textMuted} secureTextEntry value={contrasena} onChangeText={setContrasena} />

            <View style={styles.switchRow}>
              <Text style={styles.switchLabel}>Soy estudiante universitario</Text>
              <Switch
                value={esEstudiante}
                onValueChange={setEsEstudiante}
                trackColor={{ true: theme.colors.primary }}
              />
            </View>

            {esEstudiante && (
              <View style={styles.studentBox}>
                <Text style={styles.studentTitle}>Información de Estudiante</Text>
                <Text style={styles.label}>UNIVERSIDAD</Text>
                <TextInput style={styles.input} placeholder="Ej: TEC, UCR, UNA..." placeholderTextColor={theme.colors.textMuted} value={universidad} onChangeText={setUniversidad} />
                <Text style={styles.label}>CARNÉ ESTUDIANTIL</Text>
                <TextInput style={styles.input} placeholder="Ej: 2026123456" placeholderTextColor={theme.colors.textMuted} value={carnet} onChangeText={setCarnet} />
              </View>
            )}

            <TouchableOpacity style={styles.btn} onPress={handleRegister} disabled={loading}>
              {loading ? <ActivityIndicator color="#FFF" /> : <Text style={styles.btnText}>Registrarme</Text>}
            </TouchableOpacity>
          </View>

          <View style={styles.footer}>
            <Text style={styles.footerText}>¿Ya tienes cuenta? </Text>
            <TouchableOpacity onPress={() => router.replace('/login')}>
              <Text style={styles.link}>Inicia sesión aquí</Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  flex: { flex: 1 },
  container: { padding: 24, paddingTop: 40, paddingBottom: 40 },
  header: { alignItems: 'center', marginBottom: 28 },
  logo: { fontSize: 32, fontWeight: 'bold', color: theme.colors.primary, letterSpacing: 2 },
  subtitle: { fontSize: 16, color: theme.colors.textMuted, marginTop: 6 },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    padding: 24,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 10,
    elevation: 4,
  },
  label: { fontSize: 11, fontWeight: '700', color: theme.colors.textMuted, marginBottom: 6, letterSpacing: 1 },
  input: {
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    fontSize: 16,
    paddingVertical: 10,
    marginBottom: 20,
    color: theme.colors.textDark,
  },
  switchRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 },
  switchLabel: { fontSize: 15, color: theme.colors.textDark },
  studentBox: {
    backgroundColor: theme.colors.background,
    borderRadius: 10,
    padding: 16,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  studentTitle: { fontSize: 14, fontWeight: 'bold', color: theme.colors.textDark, marginBottom: 12 },
  btn: {
    backgroundColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 8,
  },
  btnText: { color: '#FFF', fontSize: 16, fontWeight: 'bold' },
  footer: { flexDirection: 'row', justifyContent: 'center', marginTop: 24 },
  footerText: { color: theme.colors.textMuted, fontSize: 14 },
  link: { color: theme.colors.primary, fontWeight: 'bold', fontSize: 14 },
});

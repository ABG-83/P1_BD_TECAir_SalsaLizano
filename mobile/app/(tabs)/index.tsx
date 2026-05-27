import React, { useState, useCallback } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, ScrollView, FlatList, Platform } from 'react-native';
import { theme } from '../../src/styles/theme';
import db from '../../src/database/db'; // Importando SQLite
import OfflineBanner from '../../components/OfflineBanner';
import { useRouter, useFocusEffect } from 'expo-router';

// Definición básica de la tabla local Vuelos
interface Vuelo {
  id: number;
  origen: string;
  destino: string;
  precio: number;
  fecha_salida: string;
  matricula_avion: string;
}

export default function SearchFlightsScreen() {
  const router = useRouter();
  const [origen, setOrigen] = useState('');
  const [destino, setDestino] = useState('');
  const [vuelosLocal, setVuelosLocal] = useState<Vuelo[]>([]);

  // Cargar vuelos EXCLUSIVAMENTE de SQLite cuando la pantalla gana el foco
  const fetchFlights = useCallback(() => {
    if (Platform.OS !== 'web' && db) {
      try {
        const results = db.getAllSync('SELECT * FROM Vuelos') as Vuelo[];
        setVuelosLocal(results);
      } catch (error) {
        console.error("Error leyendo vuelos locales:", error);
      }
    } else {
      // Dummy data si compilas en web para debugging visual
      setVuelosLocal([
        { id: 1, origen: 'SJO', destino: 'MIA', precio: 250, fecha_salida: '2026-06-01', matricula_avion: 'TEC-123' },
        { id: 2, origen: 'LIR', destino: 'JFK', precio: 300, fecha_salida: '2026-06-05', matricula_avion: 'TEC-456' },
      ]);
    }
  }, []);

  useFocusEffect(fetchFlights);

  // Lógica de filtrado 100% en el cliente JS/React Native
  const filteredVuelos = vuelosLocal.filter(v => 
    v.origen.toLowerCase().includes(origen.toLowerCase()) &&
    v.destino.toLowerCase().includes(destino.toLowerCase())
  );

  const handleSelectVuelo = (vueloId: number) => {
    router.push(`/checkout?vueloId=${vueloId}`);
  };

  const renderFlightCard = ({ item }: { item: Vuelo }) => (
    <View style={styles.flightCard}>
        <View style={styles.flightHeader}>
          <Text style={styles.flightRoute}>{item.origen} ✈️ {item.destino}</Text>
          <Text style={styles.flightPrice}>${item.precio}</Text>
        </View>
        <Text style={styles.flightDetails}>Salida: {item.fecha_salida}</Text>
        <TouchableOpacity style={styles.reserveButton} onPress={() => handleSelectVuelo(item.id)}>
          <Text style={styles.reserveButtonText}>Reservar</Text>
        </TouchableOpacity>
    </View>
  );

  return (
    <View style={styles.mainContainer}>
      {/* ⚠️ Banner Offline Global */}
      <OfflineBanner />

      <ScrollView style={styles.container} contentContainerStyle={styles.content}>
        <View style={styles.searchCard}>
          <Text style={styles.sectionTitle}>Encuentra tu vuelo</Text>
          
          <Text style={styles.label}>ORIGEN (Busca por código o ciudad)</Text>
          <TextInput 
            style={styles.minimalInput}
            placeholder="Ej: SJO"
            placeholderTextColor={theme.colors.textMuted}
            value={origen}
            onChangeText={setOrigen}
          />

          <Text style={styles.label}>DESTINO</Text>
          <TextInput 
            style={styles.minimalInput}
            placeholder="Ej: MIA"
            placeholderTextColor={theme.colors.textMuted}
            value={destino}
            onChangeText={setDestino}
          />
        </View>

        <Text style={styles.sectionTitleList}>Vuelos Disponibles ({filteredVuelos.length})</Text>
        
        {filteredVuelos.length === 0 ? (
          <Text style={styles.emptyText}>No se encontraron vuelos para esta ruta.</Text>
        ) : (
          <FlatList 
            data={filteredVuelos}
            keyExtractor={(item) => item.id.toString()}
            renderItem={renderFlightCard}
            scrollEnabled={false} // Puesto que está dentro del ScrollView principal
          />
        )}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  mainContainer: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  container: {
    flex: 1,
  },
  content: {
    padding: 20,
    paddingBottom: 40,
  },
  searchCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    padding: 20,
    marginBottom: 24,
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 10,
    elevation: 4, // Sombra suave
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 16,
    color: theme.colors.text,
  },
  sectionTitleList: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 12,
    color: theme.colors.text,
    marginLeft: 4,
  },
  label: {
    fontSize: 12,
    color: theme.colors.textMuted,
    marginBottom: 6,
    fontWeight: '600',
  },
  minimalInput: {
    borderBottomWidth: 1,
    borderBottomColor: '#E0E0E0',
    fontSize: 16,
    paddingVertical: 10,
    marginBottom: 20,
    color: theme.colors.text,
  },
  // Estilos de las tarjetas de vuelos (Limpias y modernas)
  flightCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    padding: 18,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: '#F0F0F0',
  },
  flightHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  flightRoute: {
    fontSize: 18,
    fontWeight: 'bold',
    color: theme.colors.primary,
  },
  flightPrice: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#00C853', // Verde para precios
  },
  flightDetails: {
    fontSize: 14,
    color: theme.colors.textMuted,
    marginBottom: 14,
  },
  reserveButton: {
    backgroundColor: theme.colors.primary,
    paddingVertical: 10,
    borderRadius: 8,
    alignItems: 'center',
  },
  reserveButtonText: {
    color: '#FFF',
    fontSize: 15,
    fontWeight: 'bold',
  },
  emptyText: {
    textAlign: 'center',
    color: theme.colors.textMuted,
    marginTop: 20,
  }
});

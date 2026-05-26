import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, FlatList, Image, Platform } from 'react-native';
import { theme } from '../../src/styles/theme';
import db from '../../src/database/db';
import OfflineBanner from '../../components/OfflineBanner';

// Definición de la interfaz basada en el esquema de la tabla
interface Promocion {
  id: number;
  origen: string;
  destino: string;
  precio: number;
  periodo: string;
  imagen?: string;
}

export default function PromotionsScreen() {
  const [promociones, setPromociones] = useState<Promocion[]>([]);

  useEffect(() => {
    // 100% Offline: Leer únicamente de la base local
    if (Platform.OS !== 'web' && db) {
      try {
        const results = db.getAllSync('SELECT * FROM Promociones') as Promocion[];
        setPromociones(results);
      } catch (error) {
        console.error("Error leyendo promociones locales:", error);
      }
    } else {
      // Mock de seguridad si compilas en web para debugging de UI
      setPromociones([
        { id: 1, origen: 'SJO', destino: 'MAD', precio: 150, periodo: '01 Jun - 15 Jun', imagen: 'https://images.unsplash.com/photo-1543783207-ec64e4d95325?w=500&q=80' },
        { id: 2, origen: 'LIR', destino: 'CUN', precio: 99, periodo: '10 Jul - 20 Jul' }, // Fallback sin imagen
      ]);
    }
  }, []);

  const renderPromoCard = ({ item }: { item: Promocion }) => (
    <View style={styles.card}>
      {/* Si trae imagen en Base64 o URL se dibuja, si no, se pone un header de color */}
      {item.imagen ? (
        <Image source={{ uri: item.imagen }} style={styles.image} resizeMode="cover" />
      ) : (
        <View style={styles.placeholderImage}>
          <Text style={styles.placeholderText}>✈️ Vuelo Especial</Text>
        </View>
      )}
      
      <View style={styles.cardContent}>
        <View style={styles.routeHeader}>
          <Text style={styles.routeText}>{item.origen} ✈️ {item.destino}</Text>
          <Text style={styles.priceText}>${item.precio}</Text>
        </View>
        <Text style={styles.periodText}>Válido: {item.periodo}</Text>
      </View>
    </View>
  );

  return (
    <View style={styles.mainContainer}>
      <OfflineBanner />
      
      <View style={styles.container}>
        <Text style={styles.title}>Ofertas Exclusivas</Text>
        
        {promociones.length === 0 ? (
          <Text style={styles.emptyText}>No hay promociones disponibles sincronizadas.</Text>
        ) : (
          <FlatList
            data={promociones}
            keyExtractor={(item) => item.id.toString()}
            renderItem={renderPromoCard}
            contentContainerStyle={styles.listContent}
            showsVerticalScrollIndicator={false}
          />
        )}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  mainContainer: {
    flex: 1,
    backgroundColor: '#FAFAFA',
  },
  container: {
    flex: 1,
  },
  title: {
    fontSize: 26,
    fontWeight: 'bold',
    color: '#1A1A1A',
    marginTop: 20,
    marginBottom: 15,
    paddingHorizontal: 20,
  },
  listContent: {
    paddingHorizontal: 20,
    paddingBottom: 40,
  },
  card: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    marginBottom: 24,
    overflow: 'hidden', // Importante para que la imagen no se salga de las esquinas redondeadas
    shadowColor: '#000',
    shadowOpacity: 0.08,
    shadowRadius: 10,
    elevation: 4,
    borderWidth: 1,
    borderColor: '#F0F0F0',
  },
  image: {
    width: '100%',
    height: 160,
    backgroundColor: '#E0E0E0',
  },
  placeholderImage: {
    width: '100%',
    height: 120,
    backgroundColor: '#0056B3', // Usamos el azul de tema por defecto
    alignItems: 'center',
    justifyContent: 'center',
  },
  placeholderText: {
    fontSize: 20,
    color: '#FFFFFF',
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  cardContent: {
    padding: 20,
  },
  routeHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
  },
  routeText: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#1A1A1A',
  },
  priceText: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#00C853',
  },
  periodText: {
    fontSize: 15,
    color: '#757575',
    fontWeight: '500',
  },
  emptyText: {
    textAlign: 'center',
    color: '#757575',
    marginTop: 40,
    fontSize: 16,
  }
});
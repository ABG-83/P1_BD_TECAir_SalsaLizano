import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, FlatList, Image, ActivityIndicator, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { promotionService, type Promotion } from '../../src/services/promotionService';
import { theme } from '../../src/styles/theme';

export default function PromotionsScreen() {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(true);
  const [isOffline, setIsOffline] = useState(false);

  const loadPromotions = () => {
    setLoading(true);
    setIsOffline(false);
    promotionService.getAll()
      .then(data => {
        setPromotions(data);
        if (data.length === 0) setIsOffline(false);
      })
      .catch(() => {
        setIsOffline(true);
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadPromotions();
  }, []);

  const renderCard = ({ item }: { item: Promotion }) => {
    const period = `${item.startDate?.split('T')[0] ?? ''} — ${item.endDate?.split('T')[0] ?? ''}`;
    return (
      <View style={styles.card}>
        {item.image ? (
          <Image source={{ uri: item.image }} style={styles.image} resizeMode="cover" />
        ) : (
          <View style={styles.placeholder}>
            <Text style={styles.placeholderText}>✈ Vuelo Especial</Text>
          </View>
        )}
        <View style={styles.cardContent}>
          <View style={styles.routeRow}>
            <Text style={styles.route}>{item.origin?.name} → {item.destination?.name}</Text>
            <Text style={styles.price}>${item.price}</Text>
          </View>
          <Text style={styles.period}>Válido: {period}</Text>
        </View>
      </View>
    );
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {isOffline && (
        <View style={styles.offlineBanner}>
          <Text style={styles.offlineText}>Sin conexión — mostrando datos locales</Text>
        </View>
      )}
      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={theme.colors.primary} />
        </View>
      ) : promotions.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyText}>No hay promociones disponibles.</Text>
          <TouchableOpacity style={styles.retryBtn} onPress={loadPromotions}>
            <Text style={styles.retryText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <FlatList
          data={promotions}
          keyExtractor={item => String(item.promotionId)}
          renderItem={renderCard}
          contentContainerStyle={styles.list}
          showsVerticalScrollIndicator={false}
          onRefresh={loadPromotions}
          refreshing={loading}
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: theme.colors.background },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  list: { padding: 16, paddingBottom: 40 },
  emptyText: { fontSize: 16, color: theme.colors.textMuted },
  card: {
    backgroundColor: theme.colors.white,
    borderRadius: 16,
    marginBottom: 20,
    overflow: 'hidden',
    shadowColor: '#000',
    shadowOpacity: 0.07,
    shadowRadius: 8,
    elevation: 3,
    borderWidth: 1,
    borderColor: '#F0F0F0',
  },
  image: { width: '100%', height: 160, backgroundColor: '#E0E0E0' },
  placeholder: {
    width: '100%', height: 110,
    backgroundColor: theme.colors.primary,
    alignItems: 'center', justifyContent: 'center',
  },
  placeholderText: { fontSize: 18, color: '#FFF', fontWeight: 'bold' },
  cardContent: { padding: 18 },
  routeRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  route: { fontSize: 18, fontWeight: 'bold', color: theme.colors.textDark, flex: 1 },
  price: { fontSize: 22, fontWeight: 'bold', color: '#00C853' },
  period: { fontSize: 13, color: theme.colors.textMuted },
  offlineBanner: {
    backgroundColor: '#FFF3CD',
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#FFEEBA',
  },
  offlineText: { fontSize: 13, color: '#856404', textAlign: 'center' },
  retryBtn: {
    marginTop: 16,
    backgroundColor: theme.colors.primary,
    paddingVertical: 10,
    paddingHorizontal: 28,
    borderRadius: 8,
  },
  retryText: { color: '#FFF', fontWeight: 'bold', fontSize: 14 },
});
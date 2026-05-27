import React, { useState, useCallback } from 'react';
import { View, Text, StyleSheet, FlatList, RefreshControl } from 'react-native';
import { theme } from '../../src/styles/theme';
import { getAllReservations } from '../../src/database/db';
import { useFocusEffect } from 'expo-router';

interface Reservation {
  id: number;
  flightId: string;
  userId: string;
  status: string;
  createdAt: string;
}

export default function ReservationsScreen() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [refreshing, setRefreshing] = useState(false);

  const fetchReservations = () => {
    try {
      const results = getAllReservations();
      setReservations(results as Reservation[]);
    } catch (error) {
      console.error(error);
    }
  };

  // Recargar la lista cada vez que la pantalla gana el foco
  useFocusEffect(
    useCallback(() => {
      fetchReservations();
    }, [])
  );

  const onRefresh = () => {
    setRefreshing(true);
    fetchReservations();
    setRefreshing(false);
  };

  const renderItem = ({ item }: { item: Reservation }) => {
    const isPending = item.status === 'pending' || item.status === 'pending_sync';
    
    return (
      <View style={styles.card}>
        <View style={styles.cardHeader}>
          <Text style={styles.flightId}>VUELO {item.flightId}</Text>
          <View style={[styles.badge, isPending ? styles.badgePending : styles.badgeSynced]}>
            <Text style={[styles.badgeText, isPending ? styles.badgeTextPending : styles.badgeTextSynced]}>
              {isPending ? '⏳ Pendiente Sync' : '✅ Sincronizado'}
            </Text>
          </View>
        </View>
        <Text style={styles.dateText}>Reservado el: {item.createdAt}</Text>
        <Text style={styles.userText}>Usuario: {item.userId}</Text>
      </View>
    );
  };

  return (
    <View style={styles.container}>
      {reservations.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyText}>No tienes reservas aún.</Text>
        </View>
      ) : (
        <FlatList
          data={reservations}
          keyExtractor={(item) => item.id.toString()}
          renderItem={renderItem}
          contentContainerStyle={styles.listContent}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyText: {
    color: theme.colors.textMuted,
    fontSize: 16,
  },
  listContent: {
    padding: theme.spacing.lg,
  },
  card: {
    backgroundColor: theme.colors.white,
    padding: theme.spacing.lg,
    borderRadius: theme.borderRadius.md,
    marginBottom: theme.spacing.md,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: theme.spacing.sm,
  },
  flightId: {
    fontSize: 18,
    fontWeight: 'bold',
    color: theme.colors.textDark,
  },
  badge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgePending: {
    backgroundColor: '#FFF3CD',
    borderWidth: 1,
    borderColor: '#FFEEBA',
  },
  badgeSynced: {
    backgroundColor: '#D1E7DD',
    borderWidth: 1,
    borderColor: '#BADBCC',
  },
  badgeText: {
    fontSize: 10,
    fontWeight: 'bold',
  },
  badgeTextPending: {
    color: '#856404',
  },
  badgeTextSynced: {
    color: '#0F5132',
  },
  dateText: {
    color: theme.colors.textMuted,
    fontSize: 14,
    marginBottom: 4,
  },
  userText: {
    color: theme.colors.textDark,
    fontSize: 14,
  }
});

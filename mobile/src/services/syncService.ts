import NetInfo from '@react-native-community/netinfo';
import axios from 'axios';
import { getPendingReservations, markReservationAsSynced } from '../database/db';

const API_URL = 'http://localhost:5000/api'; // Ajustar URL según el endpoint del backend

export const syncDataWithServer = async () => {
  try {
    const state = await NetInfo.fetch();
    
    // Si hay conexión a internet, procedemos a sincronizar
    if (state.isConnected) {
      console.log("Conexión detectada. Iniciando sincronización de reservaciones...");
      const pendingReservations: any[] = getPendingReservations();
      
      if (pendingReservations.length === 0) {
        console.log("No hay reservaciones pendientes de sincronización.");
        return;
      }

      for (const res of pendingReservations) {
        try {
          // Intentar enviar la reservación offline al servidor
          await axios.post(`${API_URL}/reservations`, {
            flightId: res.flightId,
            userId: res.userId
          });
          
          // Si el servidor responde exitosamente, actualizamos SQLite
          markReservationAsSynced(res.id);
          console.log(`Reservación ${res.id} sincronizada con éxito.`);
        } catch (error) {
          console.error(`Error sincronizando reservación ${res.id}:`, error);
          // Si falla una, continuamos con la siguiente
        }
      }
      console.log("Proceso de sincronización finalizado.");
    } else {
      console.log("Sin conexión. La sincronización se pospone.");
    }
  } catch (error) {
    console.error("Error en el servicio de sincronización:", error);
  }
};

// Función para inicializar un listener que escuche cambios en la red
export const initSyncListener = () => {
  return NetInfo.addEventListener(state => {
    if (state.isConnected) {
      // Cada vez que recupera la conexión, intenta sincronizar
      syncDataWithServer();
    }
  });
};

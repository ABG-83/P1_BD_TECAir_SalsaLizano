import NetInfo from '@react-native-community/netinfo';
import axios from 'axios';
import { 
  getPendingReservations, 
  updateReservationSyncStatus, 
  syncVuelosLocales, 
  syncPromocionesLocales 
} from '../database/db';

// He reemplazado la URL dinámicamente según la tarjeta de red Wi-Fi
const API_URL = 'http://192.168.68.54:5000/api';

// ==========================================
// PULL: Traer datos del servidor (C# API)
// ==========================================
export const pullDataFromServer = async () => {
  try {
    console.log("Iniciando PULL de Vuelos y Promociones...");
    
    // Traer Vuelos
    const vuelosReq = await axios.get(`${API_URL}/vuelos`);
    if (vuelosReq.data && Array.isArray(vuelosReq.data)) {
      syncVuelosLocales(vuelosReq.data);
      console.log(`PULL exitoso: ${vuelosReq.data.length} vuelos actualizados en SQLite.`);
    }

    // Traer Promociones
    const promoReq = await axios.get(`${API_URL}/promociones`);
    if (promoReq.data && Array.isArray(promoReq.data)) {
      syncPromocionesLocales(promoReq.data);
      console.log(`PULL exitoso: ${promoReq.data.length} promociones actualizadas en SQLite.`);
    }

  } catch (error) {
    console.error("Error durante el PULL desde el servidor:", error);
  }
};

// ==========================================
// PUSH: Enviar reservaciones offline al servidor
// ==========================================
export const pushPendingReservations = async () => {
  try {
    const pendingReservations: any[] = getPendingReservations();
    
    if (pendingReservations.length === 0) {
      console.log("No hay reservaciones pendientes de sincronización (PUSH).");
      return;
    }

    console.log(`Iniciando PUSH de ${pendingReservations.length} reservaciones pendientes...`);

    for (const res of pendingReservations) {
      try {
        // Enviar reservación al API. Pasamos el UUID generado localmente.
        const response = await axios.post(`${API_URL}/reservations`, {
          id: res.id,
          vuelo_id: res.vuelo_id,
          usuario_id: res.usuario_id,
          asiento: res.asiento,
          maletas: res.maletas
        });
        
        // Si el API responde 200/201 (Éxito)
        if (response.status === 200 || response.status === 201) {
          updateReservationSyncStatus(res.id, 'synced');
          console.log(`Reserva [${res.id}] sincronizada (synced) con éxito.`);
        }
      } catch (error: any) {
         // Evaluamos el tipo de error
         // 409 Conflict: El asiento ya fue tomado, 400 Bad Request: Error de tarjeta o validación de negocio
         if (error.response && (error.response.status === 409 || error.response.status === 400)) {
           console.warn(`Reserva [${res.id}] RECHAZADA. Marcando como failed_conflict.`);
           updateReservationSyncStatus(res.id, 'failed_conflict');
         } else {
           // Error de red, timeout o error no controlado (500). Se deja 'pending' para reintentar luego.
           console.error(`Error subiendo reserva [${res.id}]. Queda pendiente para reintento.`, error.message);
         }
      }
    }
  } catch (error) {
    console.error("Error general en el proceso PUSH:", error);
  }
};

// ==========================================
// ORQUESTADOR: Sync completo (Push & Pull)
// ==========================================
export const syncDataWithServer = async () => {
  try {
    const state = await NetInfo.fetch();
    
    if (state.isConnected) {
      console.log("🟢 Conexión detectada. Iniciando Sync completo...");
      
      // PUSH primero: Intentar subir lo local pendiente antes de actualizar la cartelera de vuelos
      await pushPendingReservations();
      
      // PULL después: Refrescar la base de datos local con datos nuevos de vuelos (ej: asientos ya ocupados)
      await pullDataFromServer();
      
      console.log("✅ Proceso de sincronización finalizado.");
    } else {
      console.log("🔴 Sin conexión. La sincronización se pospone.");
    }
  } catch (error) {
    console.error("Error en el orquestador de sincronización:", error);
  }
};

// ==========================================
// LISTENER: Detectar cambios en la red globalmente
// ==========================================
export const initSyncListener = () => {
  return NetInfo.addEventListener(state => {
    if (state.isConnected) {
      console.log("📡 Red reestablecida detectada por Listener. Lanzando Sync en background...");
      syncDataWithServer();
    }
  });
};

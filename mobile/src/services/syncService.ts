import NetInfo from '@react-native-community/netinfo';
import axios from 'axios';
import { Platform } from 'react-native';
import {
  getPendingReservations,
  updateReservationSyncStatus,
  syncVuelosLocales,
  syncPromocionesLocales,
  syncAeropuertosLocales,
  getFlightNumberById,
} from '../database/db';

const API_URL = process.env.EXPO_PUBLIC_API_URL || (Platform.OS === 'android' ? 'http://10.0.2.2:5102/api' : 'http://localhost:5102/api');

// ==========================================
// PULL: Traer datos del servidor (C# API)
// ==========================================
export const pullDataFromServer = async () => {
  try {
    console.log("Iniciando PULL de Aeropuertos, Vuelos y Promociones...");

    // Traer Aeropuertos
    const airportsReq = await axios.get(`${API_URL}/airports`);
    if (airportsReq.data && Array.isArray(airportsReq.data)) {
      syncAeropuertosLocales(airportsReq.data);
      console.log(`PULL exitoso: ${airportsReq.data.length} aeropuertos actualizados en SQLite.`);
    }

    // Traer Vuelos
    const vuelosReq = await axios.get(`${API_URL}/flights`);
    if (vuelosReq.data && Array.isArray(vuelosReq.data)) {
      syncVuelosLocales(vuelosReq.data);
      console.log(`PULL exitoso: ${vuelosReq.data.length} vuelos actualizados en SQLite.`);
    }

    // Traer solo promociones activas (endpoint para clientes)
    const promoReq = await axios.get(`${API_URL}/promotions/active`);
    if (promoReq.data && Array.isArray(promoReq.data)) {
      syncPromocionesLocales(promoReq.data);
      console.log(`PULL exitoso: ${promoReq.data.length} promociones activas actualizadas en SQLite.`);
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
        // Obtener el código de vuelo real (ej. TA-001) para el API
        const flightNumber = getFlightNumberById(res.vuelo_id) || `TA-${String(res.vuelo_id).padStart(3, '0')}`;

        // Enviar reservación al API. Mapeado al DTO de C# (CreateReservationDto)
        const response = await axios.post(`${API_URL}/reservations`, {
          userId: res.usuario_id,
          flightNumber: flightNumber,
          seatCount: res.cantidad_asientos || 1
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

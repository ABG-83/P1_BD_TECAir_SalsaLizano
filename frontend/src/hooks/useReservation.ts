import { useCallback, useState } from 'react';
import api from '../services/api'; 
import type { Reservation, ReservationCreate } from '../types';

export const useReservation = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ── 1. ACCIÓN: CREAR RESERVACIÓN (POST) ──
  const createReservation = useCallback(async (payload: ReservationCreate) => {
    setLoading(true);
    setError(null);

    try {
      // Mapeamos el DTO de TypeScript (snake_case) al formato que espera C# (camelCase)
      const backendPayload = {
        userId: payload.id_Usuario,
        flightNumber: payload.flightNumber ?? String(payload.num_Vuelo ?? ''),
      };

      const res = await api.post('/reservations', backendPayload);
      return res.data.reservationCode ?? res.data;
    } catch (err: any) {
      const message = err.response?.data?.title || 
                      err.message || 
                      'No se pudo completar la reservación. Verifica que el backend esté activo.';

      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // ── 2. ACCIÓN: OBTENER POR USUARIO (GET) ──
  // Útil para la vista de "Mis Viajes / Historial" de TECAir
  const getReservationsByUser = useCallback(async (userId: number): Promise<Reservation[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<Reservation[]>(`/reservations/user/${userId}`);
      return res.data;
    } catch (err: any) {
      setError(err.response?.data?.title || 'Error al cargar tus reservaciones.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  // ── 3. ACCIÓN: CANCELAR RESERVACIÓN (PATCH) ──
  const cancelReservation = useCallback(async (id: number): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await api.patch(`/reservations/${id}/cancel`);
    } catch (err: any) {
      const message = err.response?.data?.title || 'No se pudo cancelar la reservación.';
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    loading,
    error,
    createReservation,
    getReservationsByUser,
    cancelReservation,
    reset: () => setError(null),
  };
};

export default useReservation;

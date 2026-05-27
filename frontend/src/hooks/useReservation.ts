import { useCallback, useState } from 'react';
import api from '../services/api';
import type { Reservation, ReservationCreate, PaymentStatus } from '../types';

interface ReservationResponse {
  reservationCode: string;
  date: string;
  paymentState: string;
  userId: number;
  flightNumber?: string;
  userName?: string;
}

const mapPaymentState = (state: string): PaymentStatus => {
  if (state === 'Paid') return 'pagado';
  if (state === 'Failed' || state === 'Refunded') return 'cancelado';
  return 'pendiente';
};

const mapReservation = (r: ReservationResponse): Reservation => ({
  cod_Reservacion: r.reservationCode,
  fecha: r.date,
  estado_Pago: mapPaymentState(r.paymentState),
  id_Usuario: r.userId,
  num_Vuelo: 0,
  flightNumber: r.flightNumber,
  userName: r.userName ?? undefined,
});

export const useReservation = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const createReservation = useCallback(async (payload: ReservationCreate) => {
    setLoading(true);
    setError(null);
    try {
      const backendPayload = {
        userId: payload.id_Usuario,
        flightNumber: payload.flightNumber ?? String(payload.num_Vuelo ?? ''),
      };
      const res = await api.post('/reservations', backendPayload);
      return res.data.reservationCode ?? res.data;
    } catch (err) {
      const axiosErr = err as { response?: { data?: { title?: string } }; message?: string };
      const message = axiosErr.response?.data?.title || axiosErr.message || 'No se pudo completar la reservación.';
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const getReservationsByUser = useCallback(async (userId: number): Promise<Reservation[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<ReservationResponse[]>(`/reservations/user/${userId}`);
      return res.data.map(mapReservation);
    } catch (err) {
      const axiosErr = err as { response?: { data?: { title?: string } } };
      setError(axiosErr.response?.data?.title || 'Error al cargar tus reservaciones.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const getAllReservations = useCallback(async (): Promise<Reservation[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<ReservationResponse[]>('/reservations');
      return res.data.map(mapReservation);
    } catch (err) {
      const axiosErr = err as { response?: { data?: { title?: string } } };
      setError(axiosErr.response?.data?.title || 'Error al cargar las reservaciones.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const searchReservations = useCallback(async (name: string): Promise<Reservation[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<ReservationResponse[]>(`/reservations/search?name=${encodeURIComponent(name)}`);
      return res.data.map(mapReservation);
    } catch (err) {
      const axiosErr = err as { response?: { data?: { title?: string } } };
      setError(axiosErr.response?.data?.title || 'Error al buscar reservaciones.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const cancelReservation = useCallback(async (code: string): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/reservations/${code}`);
    } catch (err) {
      const axiosErr = err as { response?: { data?: { title?: string } } };
      const message = axiosErr.response?.data?.title || 'No se pudo cancelar la reservación.';
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
    getAllReservations,
    searchReservations,
    cancelReservation,
    reset: () => setError(null),
  };
};

export default useReservation;

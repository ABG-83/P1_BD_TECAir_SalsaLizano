import { useCallback, useState } from 'react';
import api from '../services/api';
import type { Baggage, BaggageCreate } from '../types';

interface BackendBaggage {
  baggageId: number;
  weight: number;
  color: string;
  reservationCode: string;
}

type AxiosErr = { response?: { data?: { title?: string; message?: string } } };

const mapBaggage = (r: BackendBaggage): Baggage => ({
  num_Maleta: r.baggageId,
  peso: Number(r.weight),
  color: r.color,
  cod_Reservacion: r.reservationCode,
  id_Usuario: 0,
});

export const useBaggage = () => {
  const [baggages, setBaggages] = useState<Baggage[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getBaggageByReservation = useCallback(async (reservationCode: string): Promise<Baggage[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<BackendBaggage[]>(`/baggage/reservation/${reservationCode}`);
      const mapped = res.data.map(mapBaggage);
      setBaggages(mapped);
      return mapped;
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || e.response?.data?.message || 'No se pudo cargar el equipaje.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const addBaggage = useCallback(async (dto: BaggageCreate): Promise<Baggage | null> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.post<BackendBaggage>('/baggage', {
        reservationCode: dto.cod_Reservacion,
        weight: dto.peso,
        color: dto.color,
      });
      const bag = mapBaggage(res.data);
      setBaggages(prev => [...prev, bag]);
      return bag;
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || e.response?.data?.message || 'No se pudo agregar la maleta.');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const reset = useCallback(() => {
    setBaggages([]);
    setError(null);
  }, []);

  return {
    baggages,
    loading,
    error,
    getBaggageByReservation,
    addBaggage,
    reset,
  };
};

export default useBaggage;

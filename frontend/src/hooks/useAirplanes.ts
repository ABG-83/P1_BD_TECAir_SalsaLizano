import { useEffect, useState } from 'react';
import api from '../services/api';

export interface AirplaneOption {
  plateNumber: string;
  passengerCapacity: number;
  seatCount: number;
}

type AxiosErr = { response?: { data?: { title?: string; message?: string } } };

export const useAirplanes = () => {
  const [airplanes, setAirplanes] = useState<AirplaneOption[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        const res = await api.get<AirplaneOption[]>('/airplanes');
        setAirplanes(res.data);
      } catch (err) {
        const e = err as AxiosErr;
        setError(e.response?.data?.title || e.response?.data?.message || 'No se pudieron cargar los aviones.');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return { airplanes, loading, error };
};

export default useAirplanes;

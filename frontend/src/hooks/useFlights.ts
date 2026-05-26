import { useCallback, useState } from 'react';
import { flightService } from '../services/flightService';
import type { Flight } from '../types';

export interface FlightSearchParams {
  origen?: number;
  destino?: number;
  fecha?: string;
}

export const useFlights = () => {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const searchFlights = useCallback(async (params: FlightSearchParams) => {
    setLoading(true);
    setError(null);

    try {
      const results = await flightService.search({
        origen: params.origen,
        destino: params.destino,
        fecha: params.fecha,
      });

      console.log(results);
      setFlights(results);
      return results;
    } catch (err) {
      const message = err instanceof Error
        ? err.message
        : 'No se pudieron cargar los vuelos. Verifica que el backend esté activo.';

      setError(message);
      setFlights([]);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    flights,
    loading,
    error,
    searchFlights,
    reset: () => {
      setFlights([]);
      setError(null);
    },
  };
};

export default useFlights;

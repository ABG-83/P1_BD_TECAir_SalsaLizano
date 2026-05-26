import { useState, useEffect } from 'react';
import api from '../services/api';

export interface AirportOption {
  airportId: number;
  name: string;
  location: string;
}

type BackendAirport = {
  airportId: number;
  name: string;
  location: string;
};

const mapAirport = (airport: BackendAirport): AirportOption => ({
  airportId: airport.airportId,
  name: airport.name,
  location: airport.location,
});

export const useAirport = () => {
  const [airports, setAirports] = useState<AirportOption[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAirports = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await api.get<BackendAirport[]>('/airports');
      setAirports(response.data.map(mapAirport));
    } catch (err: any) {
      const errorMessage = err.response?.data?.title || 'No se pudieron cargar los aeropuertos.';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAirports();
  }, []);

  return { airports, loading, error, refetch: fetchAirports };
};

export const useAirports = useAirport;

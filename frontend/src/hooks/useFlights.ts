import { useCallback, useState } from 'react';
import api from '../services/api';
import type { Flight, FlightCreate, FlightStatus } from '../types';

type BackendSearchFlight = {
  flightNumber?: string;
  departureTime: string;
  arrivalTime: string;
  status?: string | number;
  airplanePlateNumber?: string;
  originAirportId: number;
  destinationAirportId: number;
};

type BackendFlightDto = {
  flightNumber: string;
  departureTime: string;
  arrivalTime: string;
  status: string | number;
  airplanePlateNumber: string;
  origin: { airportId: number; name: string; location: string };
  destination: { airportId: number; name: string; location: string };
  passengerCapacity?: number;
  stops?: unknown[];
};

export interface FlightSearchParams {
  origen?: number;
  destino?: number;
  fecha?: string;
}

type AxiosErr = { response?: { data?: { title?: string; message?: string } } };

const parseFlightNumber = (flightNumber?: string | number): number => {
  if (typeof flightNumber === 'number') {
    return Number.isFinite(flightNumber) ? flightNumber : 0;
  }
  if (!flightNumber) return 0;
  const digits = flightNumber.match(/\d+/g)?.join('');
  return digits ? Number(digits) : 0;
};

const VALID_STATUSES: Flight['estado'][] = ['Scheduled', 'Boarding', 'Delayed', 'InAir', 'Landed', 'Cancelled'];

const mapFlightStatus = (status?: string | number): Flight['estado'] => {
  if (typeof status === 'number') {
    switch (status) {
      case 1: return 'Boarding';
      case 2: return 'Delayed';
      case 3: return 'InAir';
      case 4: return 'Landed';
      case 5: return 'Cancelled';
      default: return 'Scheduled';
    }
  }
  if (VALID_STATUSES.includes(status as Flight['estado'])) return status as Flight['estado'];
  const n = (status ?? '').toLowerCase();
  if (n.includes('board'))  return 'Boarding';
  if (n.includes('delay'))  return 'Delayed';
  if (n.includes('inair') || n.includes('in_air')) return 'InAir';
  if (n.includes('land'))   return 'Landed';
  if (n.includes('cancel')) return 'Cancelled';
  return 'Scheduled';
};

const mapFlight = (flight: BackendSearchFlight | BackendFlightDto): Flight => {
  if ('origin' in flight && 'destination' in flight) {
    return {
      num_Vuelo: parseFlightNumber(flight.flightNumber),
      hora_Salida: flight.departureTime,
      hora_Llegada: flight.arrivalTime,
      estado: mapFlightStatus(flight.status),
      matricula: flight.airplanePlateNumber,
      id_Aeropuerto_Origen: flight.origin.airportId,
      id_Aeropuerto_Destino: flight.destination.airportId,
      flightNumber: flight.flightNumber,
    };
  }
  return {
    num_Vuelo: parseFlightNumber(flight.flightNumber),
    hora_Salida: flight.departureTime,
    hora_Llegada: flight.arrivalTime,
    estado: mapFlightStatus(flight.status),
    matricula: flight.airplanePlateNumber ?? '',
    id_Aeropuerto_Origen: flight.originAirportId,
    id_Aeropuerto_Destino: flight.destinationAirportId,
    flightNumber: flight.flightNumber,
  };
};

export const useFlights = () => {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const searchFlights = useCallback(async (params: FlightSearchParams) => {
    setLoading(true);
    setError(null);
    try {
      const query = new URLSearchParams();
      if (params.origen) query.append('originId', String(params.origen));
      if (params.destino) query.append('destinationId', String(params.destino));
      if (params.fecha) query.append('date', params.fecha);
      const res = await api.get<BackendSearchFlight[]>(`/flights/search?${query.toString()}`);
      const results = res.data.map(mapFlight);
      setFlights(results);
      return results;
    } catch (err) {
      const e = err as AxiosErr & { message?: string };
      const message = e.response?.data?.title || e.message || 'No se pudieron cargar los vuelos.';
      setError(message);
      setFlights([]);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const getAllFlights = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<BackendFlightDto[]>('/flights');
      const results = res.data.map(mapFlight);
      setFlights(results);
      return results;
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'Error al cargar los vuelos.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const createFlight = useCallback(async (dto: FlightCreate): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await api.post('/flights', {
        flightNumber: dto.flightNumber ?? '',
        departureTime: dto.hora_Salida,
        arrivalTime: dto.hora_Llegada,
        airplanePlateNumber: dto.matricula,
        originAirportId: dto.id_Aeropuerto_Origen,
        destinationAirportId: dto.id_Aeropuerto_Destino,
        stopAirportIds: [],
      });
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'No se pudo crear el vuelo.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const updateFlight = useCallback(async (flightNumber: string, dto: Partial<FlightCreate>): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await api.put(`/flights/${flightNumber}`, {
        flightNumber: dto.flightNumber ?? flightNumber,
        departureTime: dto.hora_Salida,
        arrivalTime: dto.hora_Llegada,
        airplanePlateNumber: dto.matricula,
        originAirportId: dto.id_Aeropuerto_Origen,
        destinationAirportId: dto.id_Aeropuerto_Destino,
        stopAirportIds: [],
      });
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'No se pudo actualizar el vuelo.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const updateFlightStatus = useCallback(async (flightNumber: string, estado: FlightStatus): Promise<void> => {
    setError(null);
    try {
      await api.patch(`/flights/${flightNumber}/status`, { status: estado });
      setFlights(prev => prev.map(f =>
        (f.flightNumber ?? String(f.num_Vuelo)) === flightNumber ? { ...f, estado } : f
      ));
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'No se pudo cambiar el estado del vuelo.');
      throw err;
    }
  }, []);

  const deleteFlight = useCallback(async (flightNumber: string): Promise<void> => {
    setError(null);
    try {
      await api.delete(`/flights/${flightNumber}`);
      setFlights(prev => prev.filter(f => (f.flightNumber ?? String(f.num_Vuelo)) !== flightNumber));
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'No se pudo eliminar el vuelo.');
      throw err;
    }
  }, []);

  return {
    flights,
    loading,
    error,
    searchFlights,
    getAllFlights,
    createFlight,
    updateFlight,
    updateFlightStatus,
    deleteFlight,
    reset: () => {
      setFlights([]);
      setError(null);
    },
  };
};

export default useFlights;

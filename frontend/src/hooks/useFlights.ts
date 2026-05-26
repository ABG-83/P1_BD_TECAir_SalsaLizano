import { useCallback, useState } from 'react';
import api from '../services/api'; // Tu archivo de configuración de Axios
import type { Flight, FlightCreate } from '../types';

// ── 1. TIPOS DE DATOS DEL BACKEND (C#) ──
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

// ── 2. FUNCIONES DE UTILERÍA Y MAPEO ──
const parseFlightNumber = (flightNumber?: string | number): number => {
  if (typeof flightNumber === 'number') {
    return Number.isFinite(flightNumber) ? flightNumber : 0;
  }
  if (!flightNumber) return 0;
  const digits = flightNumber.match(/\d+/g)?.join('');
  return digits ? Number(digits) : 0;
};

const mapFlightStatus = (status?: string | number): Flight['estado'] => {
  if (typeof status === 'number') {
    switch (status) {
      case 1: return 'abierto';
      case 3:
      case 4: return 'cerrado';
      case 5: return 'cancelado';
      default: return 'programado';
    }
  }
  const normalized = status?.toLowerCase() ?? '';
  if (normalized.includes('board')) return 'abierto';
  if (normalized.includes('cancel')) return 'cancelado';
  if (normalized.includes('land') || normalized.includes('air')) return 'cerrado';
  return 'programado';
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

// ── 3. HOOK PERSONALIZADO CON EXTRACCIÓN DE DATOS DIRECTA ──
export const useFlights = () => {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Acción para buscar vuelos según la ruta
  const searchFlights = useCallback(async (params: FlightSearchParams) => {
    setLoading(true);
    setError(null);

    try {
      const query = new URLSearchParams();
      if (params.origen) query.append('originId', String(params.origen));
      if (params.destino) query.append('destinationId', String(params.destino));
      if (params.fecha) query.append('date', params.fecha);

      // Realizamos el request HTTP directo utilizando tu instancia de Axios
      const res = await api.get<BackendSearchFlight[]>(`/flights/search?${query.toString()}`);
      
      // Mapeamos las respuestas del API de .NET al formato de React
      const results = res.data.map(mapFlight);

      setFlights(results);
      return results;
    } catch (err: any) {
      // Capturamos el mensaje que envía tu ExceptionMiddleware de C#
      const message = err.response?.data?.title || 
                      err.message || 
                      'No se pudieron cargar los vuelos. Verifica que el backend esté activo.';

      setError(message);
      setFlights([]);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // Opcional: Métodos CRUD adicionales integrados si los ocupás para las vistas de administración
  const getAllFlights = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<BackendFlightDto[]>('/flights');
      const results = res.data.map(mapFlight);
      setFlights(results);
      return results;
    } catch (err: any) {
      setError(err.response?.data?.title || 'Error fetching flights directory.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    flights,
    loading,
    error,
    searchFlights,
    getAllFlights, // Agregado por si tu vista de admin lo ocupa
    reset: () => {
      setFlights([]);
      setError(null);
    },
  };
};

export default useFlights;

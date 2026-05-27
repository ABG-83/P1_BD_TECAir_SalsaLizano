import api from './api';
import type { Flight, FlightCreate } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

type BackendSearchFlight = {
  flightNumber?: string;
  departureTime: string;
  arrivalTime: string;
  status?: string | number;
  airplanePlateNumber?: string;
  originAirportId: number;
  destinationAirportId: number;
  price?: number;
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
  price?: number;
};

const parseFlightNumber = (flightNumber?: string | number): number => {
  if (typeof flightNumber === 'number') {
    return Number.isFinite(flightNumber) ? flightNumber : 0;
  }

  if (!flightNumber) {
    return 0;
  }

  const digits = flightNumber.match(/\d+/g)?.join('');
  return digits ? Number(digits) : 0;
};

const mapFlightStatus = (status?: string | number): Flight['estado'] => {
  if (typeof status === 'number') {
    switch (status) {
      case 1:
        return 'abierto';
      case 3:
      case 4:
        return 'cerrado';
      case 5:
        return 'cancelado';
      default:
        return 'programado';
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
      precio: flight.price ?? 0,
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
    precio: flight.price ?? 0,
  };
};

const mock = {
  getAll: () => delay([...mockDb.flights]),

  getById: (id: number) => {
    const f = mockDb.flights.find(f => f.num_Vuelo === id);
    if (!f) return Promise.reject(new Error('Not found'));
    return delay({ ...f });
  },

  search: (params: { origen?: number; destino?: number; fecha?: string }) => {
    let results = [...mockDb.flights].filter(f => f.estado !== 'cancelado');
    if (params.origen)  results = results.filter(f => f.id_Aeropuerto_Origen  === params.origen);
    if (params.destino) results = results.filter(f => f.id_Aeropuerto_Destino === params.destino);
    if (params.fecha)   results = results.filter(f => f.hora_Salida.startsWith(params.fecha!));
    return delay(results);
  },

  create: (dto: FlightCreate): Promise<number> => {
    const origin  = mockDb.airports.find(a => a.id_Aeropuerto === dto.id_Aeropuerto_Origen);
    const dest    = mockDb.airports.find(a => a.id_Aeropuerto === dto.id_Aeropuerto_Destino);
    const newFlight: Flight = {
      num_Vuelo: mockDb.nextId.vuelo++,
      ...dto,
      aeropuertoOrigen:  origin,
      aeropuertoDestino: dest,
    };
    mockDb.flights.push(newFlight);
    return delay(newFlight.num_Vuelo);
  },

  update: (flightNumber: string, dto: Partial<FlightCreate>): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.flightNumber === flightNumber || String(f.num_Vuelo) === flightNumber);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights[idx] = { ...mockDb.flights[idx], ...dto };
    return delay(undefined as void);
  },

  updateStatus: (flightNumber: string, estado: string): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.flightNumber === flightNumber || String(f.num_Vuelo) === flightNumber);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights[idx] = { ...mockDb.flights[idx], estado: estado as Flight['estado'] };
    return delay(undefined as void);
  },

  remove: (flightNumber: string): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.flightNumber === flightNumber || String(f.num_Vuelo) === flightNumber);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights.splice(idx, 1);
    return delay(undefined as void);
  },
};

const real = {
  getAll: async (): Promise<Flight[]> => {
    const res = await api.get<BackendFlightDto[]>('/flights');
    return res.data.map(mapFlight);
  },
  getById: async (id: number): Promise<Flight> => {
    const res = await api.get<BackendFlightDto>(`/flights/${id}`);
    return mapFlight(res.data);
  },
  search: async (params: { origen?: number; destino?: number; fecha?: string }): Promise<Flight[]> => {
    const query = new URLSearchParams();
    if (params.origen) query.append('originId', String(params.origen));
    if (params.destino) query.append('destinationId', String(params.destino));

    const res = await api.get<BackendSearchFlight[]>(`/flights/search?${query.toString()}`);
    return res.data.map(mapFlight);
  },
  create: async (dto: FlightCreate): Promise<number> => {
    const res = await api.post('/flights', {
      flightNumber: dto.flightNumber ?? '',
      departureTime: dto.hora_Salida,
      arrivalTime: dto.hora_Llegada,
      price: dto.precio ?? 0,
      airplanePlateNumber: dto.matricula,
      originAirportId: dto.id_Aeropuerto_Origen,
      destinationAirportId: dto.id_Aeropuerto_Destino,
      stopAirportIds: [],
    });
    return res.data.flightNumber ?? res.data;
  },
  update: async (flightNumber: string, dto: Partial<FlightCreate>): Promise<void> => {
    await api.put(`/flights/${flightNumber}`, {
      flightNumber: dto.flightNumber ?? flightNumber,
      departureTime: dto.hora_Salida,
      arrivalTime: dto.hora_Llegada,
      airplanePlateNumber: dto.matricula,
      originAirportId: dto.id_Aeropuerto_Origen,
      destinationAirportId: dto.id_Aeropuerto_Destino,
      stopAirportIds: [],
    });
  },
  updateStatus: async (flightNumber: string, estado: string): Promise<void> => {
    await api.patch(`/flights/${flightNumber}/status`, { status: estado });
  },
  remove: async (flightNumber: string): Promise<void> => {
    await api.delete(`/flights/${flightNumber}`);
  },
};

export const flightService = USE_MOCK ? mock : real;

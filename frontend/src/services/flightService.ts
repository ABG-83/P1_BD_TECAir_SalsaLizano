import api from './api';
import type { Flight, FlightCreate } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

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

  update: (id: number, dto: Partial<FlightCreate>): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.num_Vuelo === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights[idx] = { ...mockDb.flights[idx], ...dto };
    return delay(undefined as void);
  },

  updateStatus: (id: number, estado: string): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.num_Vuelo === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights[idx] = { ...mockDb.flights[idx], estado: estado as Flight['estado'] };
    return delay(undefined as void);
  },

  remove: (id: number): Promise<void> => {
    const idx = mockDb.flights.findIndex(f => f.num_Vuelo === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.flights.splice(idx, 1);
    return delay(undefined as void);
  },
};

const real = {
  getAll: async (): Promise<Flight[]> => {
    const res = await api.get('/flights');
    return res.data;
  },
  getById: async (id: number): Promise<Flight> => {
    const res = await api.get(`/flights/${id}`);
    return res.data;
  },
  search: async (params: { origen?: number; destino?: number; fecha?: string }): Promise<Flight[]> => {
    const query = new URLSearchParams();
    if (params.origen)  query.append('origen',  String(params.origen));
    if (params.destino) query.append('destino', String(params.destino));
    if (params.fecha)   query.append('fecha',   params.fecha);
    const res = await api.get(`/flights/search?${query.toString()}`);
    return res.data;
  },
  create: async (dto: FlightCreate): Promise<number> => {
    const res = await api.post('/flights', dto);
    return res.data.num_Vuelo ?? res.data;
  },
  update: async (id: number, dto: Partial<FlightCreate>): Promise<void> => {
    await api.put(`/flights/${id}`, dto);
  },
  updateStatus: async (id: number, estado: string): Promise<void> => {
    await api.patch(`/flights/${id}/status`, { estado });
  },
  remove: async (id: number): Promise<void> => {
    await api.delete(`/flights/${id}`);
  },
};

export const flightService = USE_MOCK ? mock : real;

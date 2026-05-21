import api from './api';
import type { Reservation, ReservationCreate } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mock = {
  getAll: () => delay([...mockDb.reservations]),

  getByUser: (userId: number) =>
    delay(mockDb.reservations.filter(r => r.id_Usuario === userId)),

  getById: (id: number) => {
    const r = mockDb.reservations.find(r => r.cod_Reservacion === id);
    if (!r) return Promise.reject(new Error('Not found'));
    return delay({ ...r });
  },

  create: (dto: ReservationCreate): Promise<number> => {
    const flight = mockDb.flights.find(f => f.num_Vuelo === dto.num_Vuelo);
    const newRes: Reservation = {
      cod_Reservacion: mockDb.nextId.reservacion++,
      fecha: new Date().toISOString(),
      estado_Pago: 'pendiente',
      id_Usuario: dto.id_Usuario,
      num_Vuelo: dto.num_Vuelo,
      vuelo: flight,
    };
    mockDb.reservations.push(newRes);
    return delay(newRes.cod_Reservacion);
  },

  cancel: (id: number): Promise<void> => {
    const idx = mockDb.reservations.findIndex(r => r.cod_Reservacion === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.reservations[idx] = { ...mockDb.reservations[idx], estado_Pago: 'cancelado' };
    return delay(undefined as void);
  },
};

const real = {
  getAll: async (): Promise<Reservation[]> => {
    const res = await api.get('/reservations');
    return res.data;
  },
  getByUser: async (userId: number): Promise<Reservation[]> => {
    const res = await api.get(`/reservations/user/${userId}`);
    return res.data;
  },
  getById: async (id: number): Promise<Reservation> => {
    const res = await api.get(`/reservations/${id}`);
    return res.data;
  },
  create: async (dto: ReservationCreate): Promise<number> => {
    const res = await api.post('/reservations', dto);
    return res.data.cod_Reservacion ?? res.data;
  },
  cancel: async (id: number): Promise<void> => {
    await api.patch(`/reservations/${id}/cancel`);
  },
};

export const reservationService = USE_MOCK ? mock : real;

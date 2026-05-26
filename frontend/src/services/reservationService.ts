import api from './api';
import type { Reservation, ReservationCreate, PaymentStatus } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mapPaymentState = (state: string): PaymentStatus => {
  if (state === 'Paid') return 'pagado';
  if (state === 'Failed' || state === 'Refunded') return 'cancelado';
  return 'pendiente';
};

const mapReservation = (r: any): Reservation => ({
  cod_Reservacion: r.reservationCode,
  fecha: r.date,
  estado_Pago: mapPaymentState(r.paymentState),
  id_Usuario: r.userId,
  num_Vuelo: 0,
  flightNumber: r.flightNumber,
  userName: r.userName ?? undefined,
});

const mock = {
  getAll: () => delay([...mockDb.reservations]),

  getByUser: (userId: number) =>
    delay(mockDb.reservations.filter(r => r.id_Usuario === userId)),

  getById: (id: string) => {
    const r = mockDb.reservations.find(r => r.cod_Reservacion === id);
    if (!r) return Promise.reject(new Error('Not found'));
    return delay({ ...r });
  },

  create: (dto: ReservationCreate): Promise<string> => {
    const numVuelo = dto.num_Vuelo ?? Number((dto.flightNumber ?? '').match(/\d+/g)?.join('') ?? 0);
    const flight = mockDb.flights.find(f => f.num_Vuelo === numVuelo);
    const newRes: Reservation = {
      cod_Reservacion: String(mockDb.nextId.reservacion++),
      fecha: new Date().toISOString(),
      estado_Pago: 'pendiente',
      id_Usuario: dto.id_Usuario,
      num_Vuelo: numVuelo,
      vuelo: flight,
    };
    mockDb.reservations.push(newRes);
    return delay(newRes.cod_Reservacion);
  },

  cancel: (code: string): Promise<void> => {
    const idx = mockDb.reservations.findIndex(r => r.cod_Reservacion === code);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.reservations[idx] = { ...mockDb.reservations[idx], estado_Pago: 'cancelado' };
    return delay(undefined as void);
  },
  search: (name: string) =>
    delay(mockDb.reservations.filter(r =>
      r.usuario?.nombre?.toLowerCase().includes(name.toLowerCase())
    )),
};

const real = {
  getAll: async (): Promise<Reservation[]> => {
    const res = await api.get('/reservations');
    return res.data.map(mapReservation);
  },
  getByUser: async (userId: number): Promise<Reservation[]> => {
    const res = await api.get(`/reservations/user/${userId}`);
    return res.data.map(mapReservation);
  },
  getById: async (id: string): Promise<Reservation> => {
    const res = await api.get(`/reservations/${id}`);
    return mapReservation(res.data);
  },
  create: async (dto: ReservationCreate): Promise<string> => {
    const payload = {
      userId: dto.id_Usuario,
      flightNumber: dto.flightNumber ?? String(dto.num_Vuelo ?? ''),
    };
    const res = await api.post('/reservations', payload);
    return res.data.reservationCode ?? res.data;
  },
  cancel: async (code: string): Promise<void> => {
    await api.delete(`/reservations/${code}`);
  },
  search: async (name: string): Promise<Reservation[]> => {
    const res = await api.get(`/reservations/search?name=${encodeURIComponent(name)}`);
    return res.data.map(mapReservation);
  },
};

export const reservationService = USE_MOCK ? mock : real;

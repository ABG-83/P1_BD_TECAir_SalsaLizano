import api from './api';
import type { BoardingPass, BaggageCreate, Baggage } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const SEATS = ['12A','12B','14C','15D','18A','20B','22C','25D','30A','32F'];
const GATES = ['G1','G2','G3','G4','A1','A2','B3','C4'];
const rand  = <T>(arr: T[]) => arr[Math.floor(Math.random() * arr.length)];

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 400));

const mock = {
  doCheckIn: (cod_Reservacion: number, apellidos: string): Promise<BoardingPass> => {
    const res = mockDb.reservations.find(r => r.cod_Reservacion === cod_Reservacion);
    if (!res) return Promise.reject(new Error('Reservación no encontrada'));
    const user = mockDb.users.find(u => u.id_Usuario === res.id_Usuario);
    const apellidosValidos = user?.nombre.split(' ').slice(1).join(' ') ?? '';
    if (apellidos && !apellidosValidos.toLowerCase().includes(apellidos.toLowerCase()))
      return Promise.reject(new Error('Apellidos no coinciden'));

    const existing = mockDb.boardingPasses.find(p => p.cod_Reservacion === cod_Reservacion);
    if (existing) return delay({ ...existing });

    const pass: BoardingPass = {
      id_Pase:         mockDb.nextId.pase++,
      asiento:         rand(SEATS),
      puerta_Abordaje: rand(GATES),
      hora_Impresion:  new Date().toISOString(),
      cod_Reservacion,
      num_Vuelo:       res.num_Vuelo,
    };
    mockDb.boardingPasses.push(pass);
    return delay(pass);
  },

  addBaggage: (dto: BaggageCreate): Promise<Baggage> => {
    const bag: Baggage = { num_Maleta: mockDb.nextId.maleta++, ...dto };
    mockDb.baggages.push(bag);
    return delay({ ...bag });
  },

  getBaggageByReservation: (cod_Reservacion: number) =>
    delay(mockDb.baggages.filter(b => b.cod_Reservacion === cod_Reservacion)),

  getBoardingPass: (cod_Reservacion: number) => {
    const pass = mockDb.boardingPasses.find(p => p.cod_Reservacion === cod_Reservacion);
    if (!pass) return Promise.reject(new Error('Not found'));
    return delay({ ...pass });
  },
};

const real = {
  doCheckIn: async (cod_Reservacion: number, apellidos: string): Promise<BoardingPass> => {
    const res = await api.post('/checkin', { cod_Reservacion, apellidos });
    return res.data;
  },
  addBaggage: async (dto: BaggageCreate): Promise<Baggage> => {
    const res = await api.post('/baggage', dto);
    return res.data;
  },
  getBaggageByReservation: async (cod_Reservacion: number): Promise<Baggage[]> => {
    const res = await api.get(`/baggage/reservation/${cod_Reservacion}`);
    return res.data;
  },
  getBoardingPass: async (cod_Reservacion: number): Promise<BoardingPass> => {
    const res = await api.get(`/boardingpass/${cod_Reservacion}`);
    return res.data;
  },
};

export const checkInService = USE_MOCK ? mock : real;

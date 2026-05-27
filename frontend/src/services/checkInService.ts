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
  doCheckIn: (reservationCode: string, apellidos: string): Promise<BoardingPass> => {
    const res = mockDb.reservations.find(r => r.cod_Reservacion === reservationCode);
    if (!res) return Promise.reject(new Error('Reservación no encontrada'));
    const user = mockDb.users.find(u => u.id_Usuario === res.id_Usuario);
    const apellidosValidos = user?.nombre.split(' ').slice(1).join(' ') ?? '';
    if (apellidos && !apellidosValidos.toLowerCase().includes(apellidos.toLowerCase()))
      return Promise.reject(new Error('Apellidos no coinciden'));

    const existing = mockDb.boardingPasses.find(p => p.cod_Reservacion === reservationCode);
    if (existing) return delay({ ...existing });

    const pass: BoardingPass = {
      id_Pase:         mockDb.nextId.pase++,
      asiento:         rand(SEATS),
      puerta_Abordaje: rand(GATES),
      hora_Impresion:  new Date().toISOString(),
      cod_Reservacion: reservationCode,
      num_Vuelo:       String(res.num_Vuelo),
    };
    mockDb.boardingPasses.push(pass);
    return delay(pass);
  },

  addBaggage: (dto: BaggageCreate): Promise<Baggage> => {
    const bag: Baggage = { num_Maleta: mockDb.nextId.maleta++, ...dto };
    mockDb.baggages.push(bag);
    return delay({ ...bag });
  },

  getBaggageByReservation: (reservationCode: string) =>
    delay(mockDb.baggages.filter(b => b.cod_Reservacion === reservationCode)),

  getBoardingPass: (reservationCode: string) => {
    const pass = mockDb.boardingPasses.find(p => p.cod_Reservacion === reservationCode);
    if (!pass) return Promise.reject(new Error('Not found'));
    return delay({ ...pass });
  },
};

const real = {
  doCheckIn: async (reservationCode: string, _apellidos: string): Promise<BoardingPass> => {
    // If check-in already exists, return the existing boarding pass
    try {
      const existing = await api.get(`/checkin/reservation/${reservationCode}`);
      const checkInId = existing.data?.checkInId ?? existing.data?.id;
      if (checkInId) {
        const passRes = await api.get(`/checkin/${checkInId}/boarding-pass`);
        const r = passRes.data;
        return {
          id_Pase: checkInId,
          asiento: r.seat,
          puerta_Abordaje: r.boardingGate,
          hora_Impresion: r.printTime,
          cod_Reservacion: r.reservationCode,
          num_Vuelo: r.flightNumber,
        };
      }
    } catch { /* 404 = no existing check-in, proceed to create */ }

    const randomRow = Math.floor(Math.random() * 20) + 1;
    const letters = ['A', 'B', 'C', 'D', 'F'];
    const seat = `${randomRow}${letters[Math.floor(Math.random() * letters.length)]}`;
    const gates = ['A', 'B', 'C'];
    const boardingGate = `${gates[Math.floor(Math.random() * gates.length)]}${Math.floor(Math.random() * 5) + 1}`;

    const res = await api.post('/checkin', { reservationCode, seat, boardingGate });
    const checkInId = res.data?.checkInId ?? res.data?.id;
    if (!checkInId) throw new Error('El backend no retornó un ID de check-in válido.');

    const passRes = await api.get(`/checkin/${checkInId}/boarding-pass`);
    const r = passRes.data;
    return {
      id_Pase: checkInId,
      asiento: r.seat,
      puerta_Abordaje: r.boardingGate,
      hora_Impresion: r.printTime,
      cod_Reservacion: r.reservationCode,
      num_Vuelo: r.flightNumber,
    };
  },

  addBaggage: async (dto: BaggageCreate): Promise<Baggage> => {
    const res = await api.post('/baggage', {
      reservationCode: dto.cod_Reservacion,
      weight: dto.peso,
      color: dto.color,
    });
    const r = res.data;
    return {
      num_Maleta: r.baggageId,
      peso: Number(r.weight),
      color: r.color,
      cod_Reservacion: r.reservationCode,
      id_Usuario: 0,
    };
  },

  getBaggageByReservation: async (reservationCode: string): Promise<Baggage[]> => {
    const res = await api.get(`/baggage/reservation/${reservationCode}`);
    return res.data.map((r: any) => ({
      num_Maleta: r.baggageId,
      peso: Number(r.weight),
      color: r.color,
      cod_Reservacion: r.reservationCode,
      id_Usuario: 0,
    }));
  },

  getBoardingPass: async (checkInId: number): Promise<BoardingPass> => {
    const res = await api.get(`/checkin/${checkInId}/boarding-pass`);
    const r = res.data;
    return {
      id_Pase: checkInId,
      asiento: r.seat,
      puerta_Abordaje: r.boardingGate,
      hora_Impresion: r.printTime,
      cod_Reservacion: r.reservationCode,
      num_Vuelo: r.flightNumber,
    };
  },
};

export const checkInService = USE_MOCK ? mock : real;

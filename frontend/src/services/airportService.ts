import api from './api';
import type { Airport } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'false';

type BackendAirport = {
  airportId: number;
  name: string;
  location: string;
};

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mapAirport = (airport: BackendAirport): Airport => ({
  id_Aeropuerto: airport.airportId,
  nombre: airport.name,
  ubicacion: airport.location,
});

const mock = {
  getAll: () => delay([...mockDb.airports]),
};

const real = {
  getAll: async (): Promise<Airport[]> => {
    const res = await api.get<BackendAirport[]>('/airports');
    return res.data.map(mapAirport);
  },
};

export const airportService = USE_MOCK ? mock : real;

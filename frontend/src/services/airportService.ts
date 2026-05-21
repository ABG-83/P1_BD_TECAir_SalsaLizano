import api from './api';
import type { Airport } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mock = {
  getAll: () => delay([...mockDb.airports]),
};

const real = {
  getAll: async (): Promise<Airport[]> => {
    const res = await api.get('/airports');
    return res.data;
  },
};

export const airportService = USE_MOCK ? mock : real;

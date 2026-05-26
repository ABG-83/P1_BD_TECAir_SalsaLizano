import api from './api';
import type { Promotion, PromotionCreate } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mock = {
  getAll: () => delay([...mockDb.promotions]),

  getActive: () => delay(mockDb.promotions.filter(p => p.estado_Activa)),

  create: (dto: PromotionCreate): Promise<number> => {
    const origin = mockDb.airports.find(a => a.id_Aeropuerto === dto.id_Aeropuerto_Origen);
    const dest   = mockDb.airports.find(a => a.id_Aeropuerto === dto.id_Aeropuerto_Destino);
    const promo: Promotion = {
      id_Promocion: mockDb.nextId.promocion++,
      ...dto,
      aeropuertoOrigen:  origin,
      aeropuertoDestino: dest,
    };
    mockDb.promotions.push(promo);
    return delay(promo.id_Promocion);
  },

  update: (id: number, dto: Partial<PromotionCreate>): Promise<void> => {
    const idx = mockDb.promotions.findIndex(p => p.id_Promocion === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.promotions[idx] = { ...mockDb.promotions[idx], ...dto };
    return delay(undefined as void);
  },

  remove: (id: number): Promise<void> => {
    const idx = mockDb.promotions.findIndex(p => p.id_Promocion === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.promotions.splice(idx, 1);
    return delay(undefined as void);
  },
};

const real = {
  getAll: async (): Promise<Promotion[]> => {
    const res = await api.get('/promotions');
    return res.data;
  },
  getActive: async (): Promise<Promotion[]> => {
    const res = await api.get('/promotions/active');
    return res.data;
  },
  create: async (dto: PromotionCreate): Promise<number> => {
    const res = await api.post('/promotions', dto);
    return res.data.id_Promocion ?? res.data;
  },
  update: async (id: number, dto: Partial<PromotionCreate>): Promise<void> => {
    await api.put(`/promotions/${id}`, dto);
  },
  remove: async (id: number): Promise<void> => {
    await api.delete(`/promotions/${id}`);
  },
};

export const promotionService = USE_MOCK ? mock : real;

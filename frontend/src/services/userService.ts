import api from './api';
import type { User, UserRequest } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 300));

const mock = {
  getAll: () => delay([...mockDb.users]),

  getById: (id: number) => {
    const user = mockDb.users.find(u => u.id_Usuario === id);
    if (!user) return Promise.reject(new Error('Not found'));
    return delay({ ...user });
  },

  getByEmail: (email: string) => {
    const user = mockDb.users.find(u => u.correo === email);
    if (!user) return Promise.reject(new Error('Not found'));
    return delay({ ...user });
  },

  create: (dto: UserRequest): Promise<number> => {
    const newUser: User = {
      id_Usuario: mockDb.nextId.usuario++,
      nombre: dto.nombre,
      correo: dto.correo,
      telefono: dto.telefono,
      rol: dto.rol,
      millas: dto.millas ?? 0,
      carnet: dto.carnet,
      universidad: dto.universidad,
    };
    mockDb.users.push(newUser);
    return delay(newUser.id_Usuario);
  },

  update: (id: number, dto: UserRequest): Promise<void> => {
    const idx = mockDb.users.findIndex(u => u.id_Usuario === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.users[idx] = { ...mockDb.users[idx], ...dto };
    return delay(undefined as void);
  },

  remove: (id: number): Promise<void> => {
    const idx = mockDb.users.findIndex(u => u.id_Usuario === id);
    if (idx === -1) return Promise.reject(new Error('Not found'));
    mockDb.users.splice(idx, 1);
    return delay(undefined as void);
  },
};

const real = {
  getAll: async (): Promise<User[]> => {
    const res = await api.get('/users');
    return res.data;
  },
  getById: async (id: number): Promise<User> => {
    const res = await api.get(`/users/${id}`);
    return res.data;
  },
  getByEmail: async (email: string): Promise<User> => {
    const res = await api.get(`/users/by-email?email=${encodeURIComponent(email)}`);
    return res.data;
  },
  create: async (dto: UserRequest): Promise<number> => {
    const res = await api.post('/users', dto);
    return res.data.userId;
  },
  update: async (id: number, dto: UserRequest): Promise<void> => {
    await api.put(`/users/${id}`, dto);
  },
  remove: async (id: number): Promise<void> => {
    await api.delete(`/users/${id}`);
  },
};

export const userService = USE_MOCK ? mock : real;

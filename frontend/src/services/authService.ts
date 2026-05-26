import api from './api';
import type { AuthUser, LoginRequest } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 400));

const mock = {
  login: async ({ correo }: LoginRequest): Promise<AuthUser> => {
    const user = mockDb.users.find(u => u.correo === correo);
    if (!user) throw new Error('Usuario no encontrado');
    return delay({
      id: user.id_Usuario,
      nombre: user.nombre,
      correo: user.correo,
      rol: user.rol,
      millas: user.millas,
    });
  },
};

const mapRole = (role: string): AuthUser['rol'] => {
  if (role === 'ADMIN') return 'administrador';
  return 'cliente';
};

const real = {
  login: async (credentials: LoginRequest): Promise<AuthUser> => {
    const response = await api.get(`/users/by-email?email=${encodeURIComponent(credentials.correo)}`);
    const user = response.data;
    if (!user) throw new Error('Usuario no encontrado');
    return {
      id: user.userId,
      nombre: user.fullName,
      correo: user.email,
      rol: mapRole(user.role),
      millas: user.miles,
    };
  },
};

export const authService = USE_MOCK ? mock : real;

import api from './api';
import type { AuthUser, LoginRequest } from '../types';
import { mockDb } from '../mocks/data';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

const delay = <T>(data: T): Promise<T> =>
  new Promise(resolve => setTimeout(() => resolve(data), 400));

const mock = {
  login: async ({ correo, contrasena }: LoginRequest): Promise<AuthUser> => {
    const user = mockDb.users.find(u => u.correo === correo);
    if (!user) throw new Error('Credenciales inválidas.');
    if ((user as any).contrasena && (user as any).contrasena !== contrasena)
      throw new Error('Credenciales inválidas.');
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
    const response = await api.post('/auth/login', {
      email: credentials.correo,
      password: credentials.contrasena,
    });
    const user = response.data;
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

import api from './api';

export interface AuthUser {
  id: number;
  nombre: string;
  correo: string;
  rol: string;
  millas: number;
}

const mapRole = (role: string): string => {
  if (role === 'ADMIN') return 'administrador';
  return 'cliente';
};

export const authService = {
  login: async (correo: string, contrasena: string): Promise<AuthUser> => {
    const res = await api.post('/auth/login', { email: correo, password: contrasena });
    const u = res.data;
    return {
      id: u.userId,
      nombre: u.fullName,
      correo: u.email,
      rol: mapRole(u.role),
      millas: u.miles ?? 0,
    };
  },
};

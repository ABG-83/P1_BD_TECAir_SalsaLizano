import { useCallback, useState } from 'react';
import api from '../services/api';
import type { User, UserRole } from '../types';

interface BackendUser {
  userId: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  miles: number;
  collegeIdNumber?: string | null;
  college?: string | null;
}

type AxiosErr = { response?: { data?: { title?: string; message?: string } } };

const mapRole = (role: string): UserRole => {
  if (role === 'ADMIN') return 'administrador';
  return 'cliente';
};

const mapUser = (u: BackendUser): User => ({
  id_Usuario: u.userId,
  nombre: u.fullName,
  correo: u.email,
  telefono: u.phoneNumber,
  rol: mapRole(u.role),
  millas: u.miles,
  carnet: u.collegeIdNumber ?? undefined,
  universidad: u.college ?? undefined,
});

export const useUsers = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getAllUsers = useCallback(async (): Promise<User[]> => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<BackendUser[]>('/users');
      const result = res.data.map(mapUser);
      setUsers(result);
      return result;
    } catch (err) {
      const e = err as AxiosErr;
      setError(e.response?.data?.title || 'Error al cargar los usuarios.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    users,
    loading,
    error,
    getAllUsers,
    reset: () => setError(null),
  };
};

export default useUsers;

import api from './api';

export interface UserCreate {
  nombre: string;
  telefono: string;
  correo: string;
  contrasena: string;
  carnet?: string;
  universidad?: string;
}

export const userService = {
  create: async (dto: UserCreate): Promise<void> => {
    await api.post('/users', {
      fullName: dto.nombre,
      email: dto.correo,
      phoneNumber: dto.telefono,
      password: dto.contrasena,
      collegeIdNumber: dto.carnet ?? '',
      college: dto.universidad ?? '',
    });
  },
};

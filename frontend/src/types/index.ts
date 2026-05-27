export type UserRole = 'cliente' | 'funcionario' | 'administrador';
export type FlightStatus = 'Scheduled' | 'Boarding' | 'Delayed' | 'InAir' | 'Landed' | 'Cancelled';
export type PaymentStatus = 'pendiente' | 'pagado' | 'cancelado';

export interface User {
  id_Usuario: number;
  nombre: string;
  correo: string;
  telefono: string;
  rol: UserRole;
  millas: number;
  carnet?: string;
  universidad?: string;
}

export interface UserRequest {
  nombre: string;
  correo: string;
  telefono: string;
  rol: UserRole;
  millas?: number;
  carnet?: string;
  universidad?: string;
  contrasena?: string;
}

export interface Airport {
  id_Aeropuerto: number;
  nombre: string;
  ubicacion: string;
}

export interface Airplane {
  matricula: string;
  cap_Pasajeros: number;
  num_Asientos: number;
}

export interface Flight {
  num_Vuelo: number;
  hora_Salida: string;
  hora_Llegada: string;
  estado: FlightStatus;
  matricula: string;
  id_Aeropuerto_Origen: number;
  id_Aeropuerto_Destino: number;
  aeropuertoOrigen?: Airport;
  aeropuertoDestino?: Airport;
  flightNumber?: string;
}

export interface FlightCreate {
  flightNumber?: string;
  hora_Salida: string;
  hora_Llegada: string;
  estado: FlightStatus;
  matricula: string;
  id_Aeropuerto_Origen: number;
  id_Aeropuerto_Destino: number;
}

export interface Reservation {
  cod_Reservacion: string;
  fecha: string;
  estado_Pago: PaymentStatus;
  id_Usuario: number;
  num_Vuelo: number;
  flightNumber?: string;
  userName?: string;
  vuelo?: Flight;
  usuario?: User;
}

export interface ReservationCreate {
  id_Usuario: number;
  num_Vuelo?: number;
  flightNumber?: string;
}

export interface CheckInRequest {
  cod_Reservacion: number;
  apellidos: string;
}

export interface BoardingPass {
  id_Pase: number;
  asiento: string;
  puerta_Abordaje: string;
  hora_Impresion: string;
  cod_Reservacion: string;
  num_Vuelo: string;
}

export interface Baggage {
  num_Maleta: number;
  peso: number;
  color: string;
  cod_Reservacion: string;
  id_Usuario: number;
}

export interface BaggageCreate {
  peso: number;
  color: string;
  cod_Reservacion: string;
  id_Usuario: number;
}

export interface Promotion {
  id_Promocion: number;
  precio: number;
  fecha_Inicio: string;
  fecha_Fin: string;
  imagen?: string;
  estado_Activa: boolean;
  id_Aeropuerto_Origen: number;
  id_Aeropuerto_Destino: number;
  aeropuertoOrigen?: Airport;
  aeropuertoDestino?: Airport;
}

export interface PromotionCreate {
  precio: number;
  fecha_Inicio: string;
  fecha_Fin: string;
  imagen?: string;
  estado_Activa: boolean;
  id_Aeropuerto_Origen: number;
  id_Aeropuerto_Destino: number;
}

export interface LoginRequest {
  correo: string;
  contrasena: string;
}

export interface AuthUser {
  id: number;
  nombre: string;
  correo: string;
  rol: UserRole;
  millas: number;
}

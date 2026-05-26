import type { Airport, User, Flight, Reservation, Promotion, BoardingPass, Baggage } from '../types';

export const mockAirports: Airport[] = [
  { id_Aeropuerto: 1, nombre: 'Aeropuerto Internacional Juan Santamaría', ubicacion: 'Alajuela, Costa Rica' },
  { id_Aeropuerto: 2, nombre: 'Aeropuerto Internacional Daniel Oduber',   ubicacion: 'Liberia, Costa Rica'   },
  { id_Aeropuerto: 3, nombre: 'Aeropuerto Internacional El Dorado',        ubicacion: 'Bogotá, Colombia'      },
  { id_Aeropuerto: 4, nombre: 'Aeropuerto Internacional Jorge Chávez',     ubicacion: 'Lima, Perú'            },
  { id_Aeropuerto: 5, nombre: 'Aeropuerto Internacional Tocumen',          ubicacion: 'Ciudad de Panamá, Panamá' },
  { id_Aeropuerto: 6, nombre: 'Aeropuerto Internacional Benito Juárez',    ubicacion: 'Ciudad de México, México' },
];

export const mockUsers: User[] = [
  { id_Usuario: 1,  nombre: 'Carlos Mendoza Vargas',   correo: 'carlos.mendoza@estudiantec.cr', telefono: '88001122', rol: 'cliente',        millas: 1500, carnet: 'C-2021-0001', universidad: 'Instituto Tecnológico de Costa Rica' },
  { id_Usuario: 2,  nombre: 'Ana Sofía Jiménez Rojas', correo: 'ana.jimenez@estudiantec.cr',    telefono: '88002233', rol: 'cliente',        millas: 800,  carnet: 'C-2022-0045', universidad: 'Instituto Tecnológico de Costa Rica' },
  { id_Usuario: 3,  nombre: 'Diego Hernández Mora',    correo: 'diego.hernandez@ucr.ac.cr',     telefono: '88003344', rol: 'cliente',        millas: 200,  carnet: 'B95432',      universidad: 'Universidad de Costa Rica' },
  { id_Usuario: 4,  nombre: 'Valeria Torres Campos',   correo: 'valeria.torres@una.ac.cr',      telefono: '88004455', rol: 'cliente',        millas: 3200, carnet: 'UNA-20190034',universidad: 'Universidad Nacional de Costa Rica' },
  { id_Usuario: 5,  nombre: 'Luis Fernando Quesada',   correo: 'luis.quesada@gmail.com',        telefono: '88005566', rol: 'cliente',        millas: 500  },
  { id_Usuario: 6,  nombre: 'María Elena Castillo',    correo: 'maria.castillo@gmail.com',      telefono: '88006677', rol: 'cliente',        millas: 950  },
  { id_Usuario: 7,  nombre: 'Roberto Alvarado Pérez',  correo: 'roberto.alvarado@hotmail.com',  telefono: '88007788', rol: 'cliente',        millas: 100  },
  { id_Usuario: 8,  nombre: 'Daniela Núñez Solís',     correo: 'daniela.nunez@outlook.com',     telefono: '88008899', rol: 'cliente',        millas: 2700 },
  { id_Usuario: 9,  nombre: 'Andrés Calvo Blanco',     correo: 'andres.calvo@gmail.com',        telefono: '88009900', rol: 'cliente',        millas: 0    },
  { id_Usuario: 10, nombre: 'Patricia Solano Vega',    correo: 'patricia.solano@tecair.cr',     telefono: '22001100', rol: 'funcionario',    millas: 0    },
  { id_Usuario: 11, nombre: 'Marco Rodríguez Fallas',  correo: 'marco.rodriguez@tecair.cr',     telefono: '22002200', rol: 'funcionario',    millas: 0    },
  { id_Usuario: 12, nombre: 'Admin TECAir',             correo: 'admin@tecair.cr',               telefono: '22003300', rol: 'administrador',  millas: 0    },
];

export const mockFlights: Flight[] = [
  {
    num_Vuelo: 101, hora_Salida: '2026-07-15T08:00:00', hora_Llegada: '2026-07-15T11:30:00',
    estado: 'abierto', matricula: 'TEC-001', id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 5,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[4],
  },
  {
    num_Vuelo: 102, hora_Salida: '2026-07-16T14:30:00', hora_Llegada: '2026-07-17T06:00:00',
    estado: 'abierto', matricula: 'TEC-002', id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 3,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[2],
  },
  {
    num_Vuelo: 103, hora_Salida: '2026-07-18T09:00:00', hora_Llegada: '2026-07-18T12:00:00',
    estado: 'programado', matricula: 'TEC-003', id_Aeropuerto_Origen: 2, id_Aeropuerto_Destino: 5,
    aeropuertoOrigen: mockAirports[1], aeropuertoDestino: mockAirports[4],
  },
  {
    num_Vuelo: 104, hora_Salida: '2026-07-20T07:00:00', hora_Llegada: '2026-07-20T10:30:00',
    estado: 'abierto', matricula: 'TEC-004', id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 4,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[3],
  },
  {
    num_Vuelo: 105, hora_Salida: '2026-06-10T06:00:00', hora_Llegada: '2026-06-10T09:00:00',
    estado: 'cerrado', matricula: 'TEC-001', id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 6,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[5],
  },
];

export const mockReservations: Reservation[] = [
  { cod_Reservacion: '1001', fecha: '2026-06-01T10:00:00', estado_Pago: 'pagado',    id_Usuario: 1, num_Vuelo: 101, vuelo: mockFlights[0] },
  { cod_Reservacion: '1002', fecha: '2026-06-02T11:00:00', estado_Pago: 'pendiente', id_Usuario: 1, num_Vuelo: 102, vuelo: mockFlights[1] },
  { cod_Reservacion: '1003', fecha: '2026-06-03T12:00:00', estado_Pago: 'pagado',    id_Usuario: 5, num_Vuelo: 101, vuelo: mockFlights[0] },
  { cod_Reservacion: '1004', fecha: '2026-06-04T09:00:00', estado_Pago: 'cancelado', id_Usuario: 6, num_Vuelo: 103, vuelo: mockFlights[2] },
];

export const mockPromotions: Promotion[] = [
  {
    id_Promocion: 1, precio: 125.00, fecha_Inicio: '2026-09-01', fecha_Fin: '2026-12-31',
    imagen: 'https://images.unsplash.com/photo-1514214246283-d427a95c5d2f?w=800&q=80',
    estado_Activa: true, id_Aeropuerto_Origen: 2, id_Aeropuerto_Destino: 5,
    aeropuertoOrigen: mockAirports[1], aeropuertoDestino: mockAirports[4],
  },
  {
    id_Promocion: 2, precio: 399.00, fecha_Inicio: '2026-08-01', fecha_Fin: '2026-11-30',
    imagen: 'https://images.unsplash.com/photo-1539037116277-4db20202d03e?w=800&q=80',
    estado_Activa: true, id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 3,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[2],
  },
  {
    id_Promocion: 3, precio: 210.00, fecha_Inicio: '2026-07-01', fecha_Fin: '2026-08-31',
    imagen: 'https://images.unsplash.com/photo-1436491865332-7a61a109cc05?w=800&q=80',
    estado_Activa: false, id_Aeropuerto_Origen: 1, id_Aeropuerto_Destino: 6,
    aeropuertoOrigen: mockAirports[0], aeropuertoDestino: mockAirports[5],
  },
];

export const mockBoardingPasses: BoardingPass[] = [
  { id_Pase: 1, asiento: '12A', puerta_Abordaje: 'G4', hora_Impresion: '2026-07-15T06:30:00', cod_Reservacion: '1001', num_Vuelo: '101' },
];

export const mockBaggages: Baggage[] = [
  { num_Maleta: 1, peso: 23.0, color: 'Negro',  cod_Reservacion: '1001', id_Usuario: 1 },
  { num_Maleta: 2, peso: 15.5, color: 'Azul',   cod_Reservacion: '1001', id_Usuario: 1 },
];

let nextId = {
  usuario: 13, vuelo: 106, reservacion: 1005, pase: 2, maleta: 3, promocion: 4,
};

export const mockDb = {
  airports:     mockAirports,
  users:        mockUsers,
  flights:      mockFlights,
  reservations: mockReservations,
  promotions:   mockPromotions,
  boardingPasses: mockBoardingPasses,
  baggages:     mockBaggages,
  nextId,
};

import api from './api';

export interface Reservation {
  cod_Reservacion: string;
  flightNumber: string;
  fecha: string;
  estado_Pago: 'pendiente' | 'pagado' | 'cancelado';
}

const mapPayment = (state: string): Reservation['estado_Pago'] => {
  if (state === 'Paid') return 'pagado';
  if (state === 'Failed' || state === 'Refunded') return 'cancelado';
  return 'pendiente';
};

const mapReservation = (r: any): Reservation => ({
  cod_Reservacion: r.reservationCode,
  flightNumber: r.flightNumber ?? '',
  fecha: r.date,
  estado_Pago: mapPayment(r.paymentState),
});

export const reservationService = {
  getByUser: async (userId: number): Promise<Reservation[]> => {
    const res = await api.get(`/reservations/user/${userId}`);
    return res.data.map(mapReservation);
  },

  create: async (userId: number, flightNumber: string): Promise<string> => {
    const res = await api.post('/reservations', { userId, flightNumber });
    return res.data.reservationCode ?? res.data;
  },

  cancel: async (code: string): Promise<void> => {
    await api.delete(`/reservations/${code}`);
  },
};

import api from './api';

export interface BoardingPass {
  checkInId: number;
  num_Vuelo: string;
  cod_Reservacion: string;
  asiento: string;
  puerta_Abordaje: string;
  hora_Impresion: string;
}

export const checkInService = {
  doCheckIn: async (reservationCode: string, apellidos: string): Promise<BoardingPass> => {
    const res = await api.post('/checkin', {
      reservationCode,
      lastName: apellidos,
    });
    const d = res.data;
    return {
      checkInId: d.checkInId,
      num_Vuelo: d.flightNumber ?? d.num_Vuelo ?? '',
      cod_Reservacion: d.reservationCode ?? d.cod_Reservacion ?? reservationCode,
      asiento: d.seat ?? d.asiento ?? '',
      puerta_Abordaje: d.boardingGate ?? d.puerta_Abordaje ?? '',
      hora_Impresion: d.checkInTime ?? d.hora_Impresion ?? new Date().toISOString(),
    };
  },

  getBoardingPass: async (checkInId: number): Promise<BoardingPass> => {
    const res = await api.get(`/checkin/${checkInId}/boarding-pass`);
    const d = res.data;
    return {
      checkInId: d.checkInId ?? checkInId,
      num_Vuelo: d.flightNumber ?? '',
      cod_Reservacion: d.reservationCode ?? '',
      asiento: d.seat ?? '',
      puerta_Abordaje: d.boardingGate ?? '',
      hora_Impresion: d.checkInTime ?? new Date().toISOString(),
    };
  },
};

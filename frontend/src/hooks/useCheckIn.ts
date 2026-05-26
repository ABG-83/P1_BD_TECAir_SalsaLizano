import { useState, useCallback } from 'react';
import api from '../services/api';
import type { BoardingPass } from '../types';

interface BackendBoardingPassDto {
  checkInId: number;
  seat: string;
  boardingGate: string;
  printTime: string;
  reservationCode: string;
  flightNumber: string;
}

export const useCheckIn = () => {
  const [boardingPass, setBoardingPass] = useState<BoardingPass | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const executeCheckIn = useCallback(async (reservationCode: string, lastName: string) => {
    setLoading(true);
    setError(null);
    setBoardingPass(null);

    try {
      const randomRow = Math.floor(Math.random() * 20) + 1;
      const letters = ['A', 'B', 'C', 'D', 'F'];
      const seatString = `${randomRow}${letters[Math.floor(Math.random() * letters.length)]}`;
      const gates = ['A', 'B', 'C'];
      const randomGate = `${gates[Math.floor(Math.random() * gates.length)]}${Math.floor(Math.random() * 5) + 1}`;

      const createResponse = await api.post('/checkin', {
        reservationCode,
        seat: seatString,
        boardingGate: randomGate,
      });

      const checkInId = createResponse.data?.checkInId ?? createResponse.data?.id;
      if (!checkInId) throw new Error('El backend no retornó un ID de check-in válido.');

      const passResponse = await api.get<BackendBoardingPassDto>(`/checkin/${checkInId}/boarding-pass`);
      const backendPass = passResponse.data;

      const mappedPass: BoardingPass = {
        id_Pase: checkInId,
        num_Vuelo: backendPass.flightNumber,
        cod_Reservacion: backendPass.reservationCode,
        asiento: backendPass.seat,
        puerta_Abordaje: backendPass.boardingGate,
        hora_Impresion: backendPass.printTime,
      };

      setBoardingPass(mappedPass);
      return mappedPass;

    } catch (err: any) {
      if (err.response?.status === 404) {
        setError(`La reservación '${reservationCode}' no fue encontrada o no está en estado 'Paid'.`);
        return null;
      }
      const backendMessage = err.response?.data?.message || err.response?.data?.title;
      setError(backendMessage || 'Ocurrió un error inesperado al procesar el check-in.');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const resetCheckIn = useCallback(() => {
    setBoardingPass(null);
    setError(null);
  }, []);

  return {
    boardingPass,
    loading,
    error,
    executeCheckIn,
    resetCheckIn
  };
};

export default useCheckIn;

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

  const executeCheckIn = useCallback(async (reservationCode: string, _lastName: string) => {
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

    } catch (err) {
      const axiosErr = err as { response?: { status?: number; data?: { message?: string; title?: string } } };
      const backendMsg = axiosErr.response?.data?.title ?? axiosErr.response?.data?.message ?? '';

      // Reservation already has a check-in: extract the existing ID and fetch the boarding pass
      if (axiosErr.response?.status === 400) {
        const idMatch = backendMsg.match(/ID:\s*(\d+)/i);
        if (idMatch) {
          try {
            const existingId = Number(idMatch[1]);
            const passRes = await api.get<BackendBoardingPassDto>(`/checkin/${existingId}/boarding-pass`);
            const p = passRes.data;
            const existing: BoardingPass = {
              id_Pase: existingId,
              num_Vuelo: p.flightNumber,
              cod_Reservacion: p.reservationCode,
              asiento: p.seat,
              puerta_Abordaje: p.boardingGate,
              hora_Impresion: p.printTime,
            };
            setBoardingPass(existing);
            return existing;
          } catch {
            // fall through to generic error
          }
        }
      }

      if (axiosErr.response?.status === 404) {
        setError(`La reservación '${reservationCode}' no fue encontrada o no está en estado 'Paid'.`);
        return null;
      }
      setError(backendMsg || 'Ocurrió un error inesperado al procesar el check-in.');
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

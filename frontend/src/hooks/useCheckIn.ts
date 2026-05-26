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

  const executeCheckIn = useCallback(async (reservationId: number, lastName: string) => {
    setLoading(true);
    setError(null);
    setBoardingPass(null);

    try {
      // 1. Generar asiento válido
      const randomRow = Math.floor(Math.random() * 20) + 1;
      const letters = ['A', 'B', 'C', 'D', 'F'];
      const randomLetter = letters[Math.floor(Math.random() * letters.length)];
      const seatString = `${randomRow}${randomLetter}`;
      
      // 2. 💡 CORRECCIÓN DE PUERTA: Formato corto (ej: "A3") para no romper el [MaxLength(10)] del DTO de C#
      const gates = ['A', 'B', 'C'];
      const randomGate = `${gates[Math.floor(Math.random() * gates.length)]}${Math.floor(Math.random() * 5) + 1}`;

      const payload = {
        reservationCode: String(reservationId),
        seat: seatString,
        boardingGate: randomGate
      };

      console.log('Enviando payload verificado a TECAir Core:', payload);

      // 3. Registrar transacción de Check-In
      const createResponse = await api.post('/checkin', payload);
      
      const checkInId = createResponse.data?.checkInId ?? 
                        createResponse.data?.CheckInId ?? 
                        createResponse.data?.id;

      if (!checkInId) {
        throw new Error('El backend procesó el chequeo pero no retornó un ID válido.');
      }

      // 4. Obtener Pase de abordaje final
      const passResponse = await api.get<BackendBoardingPassDto>(`/checkin/${checkInId}/boarding-pass`);
      const backendPass = passResponse.data;

      const mappedPass: BoardingPass = {
        num_Vuelo: backendPass.flightNumber,
        cod_Reservacion: Number(backendPass.reservationCode),
        asiento: backendPass.seat,
        puerta_Abordaje: backendPass.boardingGate,
        hora_Impresion: backendPass.printTime
      };

      setBoardingPass(mappedPass);
      return mappedPass;

    } catch (err: any) {
      console.error('Check-in operational routine failure:', err);

      // 💡 MANEJO DE RESPUESTA 404 RELACIONAL
      if (err.response?.status === 404) {
        setError(`La reservación #${reservationId} no fue encontrada o no se encuentra en estado 'PAGADA'.`);
        return null;
      }

      const backendMessage = err.response?.data?.message || err.response?.data?.title;
      setError(backendMessage || 'Ocurrió un error inesperado al procesar el pre-chequeo.');
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

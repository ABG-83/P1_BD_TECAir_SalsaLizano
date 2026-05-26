import api from './api';

export interface PaymentRequest {
  reservationCode: string;
  amount: number;
  cardNumber: string;
  cardholderName: string;
  expirationDate: string;
  cvv: string;
}

export const paymentService = {
  process: async (dto: PaymentRequest): Promise<void> => {
    await api.post('/payments/process', {
      reservationCode: dto.reservationCode,
      amount: dto.amount,
      cardNumber: dto.cardNumber,
      cardholderName: dto.cardholderName,
      expirationDate: dto.expirationDate,
      cvv: dto.cvv,
    });
  },
};

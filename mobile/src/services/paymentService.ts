import api from './api';

export const paymentService = {
  process: async (dto: {
    reservationCode: string;
    amount: number;
    cardNumber: string;
    cardholderName: string;
    expirationDate: string;
    cvv: string;
  }): Promise<void> => {
    await api.post('/payments/process', dto);
  },
};

import flightData from '../mocks/flights.json';

// Servicio de vuelos utilizando mocks temporalmente
export const flightService = {
  getFlights: async () => {
    // Simular un retardo de red
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(flightData);
      }, 500);
    });
  }
};

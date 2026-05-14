import { useState, useEffect } from 'react';
import { Container } from 'react-bootstrap';
import FlightCard from '../components/FlightCard';
import { flightService } from '../services/flightService';

const ReservationView = () => {
  const [flights, setFlights] = useState<any[]>([]);

  useEffect(() => {
    const loadFlights = async () => {
      const data = await flightService.getFlights();
      setFlights(data as any[]);
    };
    loadFlights();
  }, []);

  const handleReserve = (flightId: string) => {
    alert(`Reservando vuelo: ${flightId}`);
  };

  return (
    <Container className="py-4">
      <h3 className="mb-4 fw-bold">Resultados de Búsqueda</h3>
      <p className="text-muted mb-4">Selecciona el vuelo que mejor se adapte a tu horario.</p>
      
      {flights.length === 0 ? (
        <div className="text-center py-5">
          <div className="spinner-border text-dark" role="status">
            <span className="visually-hidden">Cargando vuelos...</span>
          </div>
        </div>
      ) : (
        flights.map((flight) => (
          <FlightCard key={flight.id} flight={flight} onReserve={handleReserve} />
        ))
      )}
    </Container>
  );
};

export default ReservationView;

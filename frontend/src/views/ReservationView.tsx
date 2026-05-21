import { useState, useEffect } from 'react';
import { Container, Alert, Button, Modal } from 'react-bootstrap';
import { useSearchParams, useNavigate } from 'react-router-dom';
import FlightCard from '../components/FlightCard';
import { flightService } from '../services/flightService';
import { reservationService } from '../services/reservationService';
import { useAuth } from '../context/AuthContext';
import type { Flight } from '../types';

const ReservationView = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedFlight, setSelectedFlight] = useState<Flight | null>(null);
  const [reserving, setReserving] = useState(false);
  const [success, setSuccess] = useState('');

  const origen = searchParams.get('origen') ?? '';
  const destino = searchParams.get('destino') ?? '';
  const fecha = searchParams.get('fecha') ?? '';

  useEffect(() => {
    setLoading(true);
    setError('');
    flightService.search({
      origen: origen ? Number(origen) : undefined,
      destino: destino ? Number(destino) : undefined,
      fecha: fecha || undefined,
    })
      .then(setFlights)
      .catch(() => setError('No se pudieron cargar los vuelos. Verifica que el backend esté activo.'))
      .finally(() => setLoading(false));
  }, [origen, destino, fecha]);

  const handleReserve = (flightId: number) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    const flight = flights.find(f => f.num_Vuelo === flightId);
    if (flight) setSelectedFlight(flight);
  };

  const confirmReservation = async () => {
    if (!selectedFlight || !user) return;
    setReserving(true);
    try {
      const cod = await reservationService.create({
        id_Usuario: user.id,
        num_Vuelo: selectedFlight.num_Vuelo,
      });
      setSuccess(`¡Reservación #${cod} confirmada! Revisa tus reservaciones.`);
      setSelectedFlight(null);
    } catch {
      setError('No se pudo completar la reservación.');
    } finally {
      setReserving(false);
    }
  };

  return (
    <Container className="py-4">
      <h3 className="mb-2 fw-bold">Resultados de Búsqueda</h3>
      <p className="text-muted mb-4">Selecciona el vuelo que mejor se adapte a tu horario.</p>

      {success && <Alert variant="success" dismissible onClose={() => setSuccess('')}>{success}</Alert>}
      {error && <Alert variant="warning">{error}</Alert>}

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-dark" role="status">
            <span className="visually-hidden">Cargando vuelos...</span>
          </div>
        </div>
      ) : flights.length === 0 && !error ? (
        <div className="text-center py-5 text-muted">
          <p className="fs-5">No se encontraron vuelos para los criterios seleccionados.</p>
          <Button variant="outline-dark" onClick={() => navigate('/')}>Volver a buscar</Button>
        </div>
      ) : (
        flights.map(flight => (
          <FlightCard key={flight.num_Vuelo} flight={flight} onReserve={handleReserve} />
        ))
      )}

      <Modal show={!!selectedFlight} onHide={() => setSelectedFlight(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">Confirmar Reservación</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedFlight && (
            <>
              <p className="mb-1"><strong>Vuelo:</strong> #{selectedFlight.num_Vuelo}</p>
              <p className="mb-1"><strong>Salida:</strong> {new Date(selectedFlight.hora_Salida).toLocaleString('es-CR')}</p>
              <p className="mb-1"><strong>Llegada:</strong> {new Date(selectedFlight.hora_Llegada).toLocaleString('es-CR')}</p>
              <p className="mb-0"><strong>Pasajero:</strong> {user?.nombre}</p>
            </>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={() => setSelectedFlight(null)}>Cancelar</Button>
          <Button variant="dark" onClick={confirmReservation} disabled={reserving}>
            {reserving ? 'Procesando...' : 'Confirmar'}
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default ReservationView;

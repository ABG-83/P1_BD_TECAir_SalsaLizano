import { useState, useEffect } from 'react';
import { Container, Alert, Button, Modal } from 'react-bootstrap';
import { useSearchParams, useNavigate } from 'react-router-dom';
import FlightCard from '../components/FlightCard';
import { useReservation } from '../hooks/useReservation';
import { useAuth } from '../context/AuthContext';
import { useFlights } from '../hooks/useFlights';
import type { Flight } from '../types';

const ReservationView = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();

  const { flights, loading, error: searchError, searchFlights } = useFlights();
  const { createReservation, loading: reserving } = useReservation();

  const [reservationError, setReservationError] = useState('');
  const [selectedFlight, setSelectedFlight] = useState<Flight | null>(null);
  const [success, setSuccess] = useState('');

  const origen = searchParams.get('origen') ?? '';
  const destino = searchParams.get('destino') ?? '';
  const fecha = searchParams.get('fecha') ?? '';

  useEffect(() => {
    setReservationError('');
    setSuccess('');

    // Prevenimos enviar ceros o valores nulos que rompan la validación de C# (Int32)
    const originId = origen ? Number(origen) : undefined;
    const destinationId = destino ? Number(destino) : undefined;

    // Solo disparamos la petición si los IDs son numéricamente válidos
    if (originId && destinationId) {
      searchFlights({
        origen: originId,
        destino: destinationId,
        fecha: fecha || undefined,
      }).catch((err) => {
        console.error("Error capturado en la vista de reservación:", err);
      });
    }
  }, [origen, destino, fecha, searchFlights]);

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

    setReservationError('');
    try {
      // Consumo directo de la acción expuesta por el Hook
      const cod = await createReservation({
        id_Usuario: user.id,
        flightNumber: selectedFlight.flightNumber,
        num_Vuelo: selectedFlight.num_Vuelo,
      });

      setSuccess(`¡Reservación #${cod} confirmada! Revisa tus reservaciones.`);
      setSelectedFlight(null);
    } catch (err: any) {
      const msg = err.response?.data?.title || 'No se pudo completar la reservación en el sistema central de TECAir.';
      setReservationError(msg);
    }
  };

  return (
    <Container className="py-4">
      <h3 className="mb-2 fw-bold">Resultados de Búsqueda</h3>
      <p className="text-muted mb-4">Selecciona el vuelo que mejor se adapte a tu horario.</p>

      {success && <Alert variant="success" dismissible onClose={() => setSuccess('')}>{success}</Alert>}
      {searchError && <Alert variant="warning">{searchError}</Alert>}
      {reservationError && <Alert variant="warning">{reservationError}</Alert>}

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-dark" role="status">
            <span className="visually-hidden">Cargando vuelos...</span>
          </div>
        </div>
      ) : flights.length === 0 && !searchError ? (
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
          <Button variant="outline-secondary" onClick={() => setSelectedFlight(null)} disabled={reserving}>Cancelar</Button>
          <Button variant="dark" onClick={confirmReservation} disabled={reserving}>
            {reserving ? 'Procesando...' : 'Confirmar'}
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default ReservationView;

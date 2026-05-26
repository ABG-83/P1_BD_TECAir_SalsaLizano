import { useState, useEffect } from 'react';
import { Container, Table, Badge, Spinner, Alert, Button } from 'react-bootstrap';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { reservationService } from '../services/reservationService';
import type { Reservation } from '../types';

const PAYMENT_BADGE: Record<string, string> = {
  pendiente: 'warning',
  pagado:    'success',
  cancelado: 'danger',
};

const MyReservationsView = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancelling, setCancelling] = useState<string | null>(null);

  const isAdmin = user?.rol === 'administrador';

  const load = () => {
    if (!user) return;
    setLoading(true);
    const fetch = isAdmin ? reservationService.getAll() : reservationService.getByUser(user.id);
    fetch
      .then(setReservations)
      .catch(() => setError('No se pudieron cargar las reservaciones.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, [user]);

  const handleCancel = async (cod: string) => {
    if (!confirm('¿Seguro que deseas cancelar esta reservación?')) return;
    setCancelling(cod);
    try {
      await reservationService.cancel(cod);
      load();
    } catch {
      setError('No se pudo cancelar la reservación.');
    } finally {
      setCancelling(null);
    }
  };

  return (
    <Container className="py-4">
      <h3 className="fw-bold mb-1">Mis Reservaciones</h3>
      <p className="text-muted mb-4">Historial de vuelos reservados por {user?.nombre}.</p>

      {error && <Alert variant="danger">{error}</Alert>}

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : reservations.length === 0 ? (
        <div className="text-center py-5 text-muted">
          <p className="fs-5">No tienes reservaciones aún.</p>
          <Button variant="dark" href="/">Buscar vuelos</Button>
        </div>
      ) : (
        <div className="bg-white rounded shadow-sm overflow-hidden">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>#Reservación</th>
                <th>Vuelo</th>
                <th>Fecha Reserva</th>
                <th>Estado Pago</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {reservations.map(r => (
                <tr key={r.cod_Reservacion}>
                  <td className="fw-bold">#{r.cod_Reservacion}</td>
                  <td>Vuelo {r.flightNumber ?? `#${r.num_Vuelo}`}</td>
                  <td>{new Date(r.fecha).toLocaleDateString('es-CR')}</td>
                  <td>
                    <Badge bg={PAYMENT_BADGE[r.estado_Pago] ?? 'secondary'}>
                      {r.estado_Pago}
                    </Badge>
                  </td>
                  <td className="d-flex gap-2">
                    {r.estado_Pago === 'pendiente' && (
                      <Button
                        variant="dark"
                        size="sm"
                        onClick={() => navigate(`/pago?cod=${r.cod_Reservacion}`)}
                      >
                        Pagar
                      </Button>
                    )}
                    {r.estado_Pago === 'pendiente' && (
                      <Button
                        variant="outline-danger"
                        size="sm"
                        disabled={cancelling === r.cod_Reservacion}
                        onClick={() => handleCancel(r.cod_Reservacion)}
                      >
                        {cancelling === r.cod_Reservacion ? '...' : 'Cancelar'}
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}
    </Container>
  );
};

export default MyReservationsView;

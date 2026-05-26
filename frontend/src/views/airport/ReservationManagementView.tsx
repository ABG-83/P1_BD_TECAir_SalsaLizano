import { useState, useEffect } from 'react';
import { Table, Badge, Button, Form, InputGroup, Spinner, Alert, Modal, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { reservationService } from '../../services/reservationService';
import { checkInService } from '../../services/checkInService';
import type { Reservation, Baggage } from '../../types';

const PAYMENT_BADGE: Record<string, string> = {
  pendiente: 'warning',
  pagado:    'success',
  cancelado: 'danger',
};

const ReservationManagementView = () => {
  const navigate = useNavigate();
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [searching, setSearching] = useState(false);

  const [baggageModal, setBaggageModal] = useState<{ cod: string; userName?: string } | null>(null);
  const [baggages, setBaggages] = useState<Baggage[]>([]);
  const [baggageLoading, setBaggageLoading] = useState(false);

  const load = () => {
    setLoading(true);
    reservationService.getAll()
      .then(setReservations)
      .catch(() => setError('No se pudieron cargar las reservaciones.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!search.trim()) { load(); return; }
    setSearching(true);
    setError('');
    try {
      const results = await reservationService.search(search.trim());
      setReservations(results);
    } catch {
      setError('Error al buscar reservaciones.');
    } finally {
      setSearching(false);
    }
  };

  const openBaggage = async (cod: string, userName?: string) => {
    setBaggageModal({ cod, userName });
    setBaggageLoading(true);
    setBaggages([]);
    try {
      const bags = await checkInService.getBaggageByReservation(cod);
      setBaggages(bags);
    } catch {
      setBaggages([]);
    } finally {
      setBaggageLoading(false);
    }
  };

  const baggageFee = (count: number) => {
    if (count <= 1) return 0;
    if (count === 2) return 50;
    return 50 + (count - 2) * 75;
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 className="fw-bold mb-1">Reservaciones</h3>
          <p className="text-muted mb-0">Gestión de reservaciones y equipaje.</p>
        </div>
      </div>

      {error && <Alert variant="danger" dismissible onClose={() => setError('')}>{error}</Alert>}

      <Form onSubmit={handleSearch} className="mb-3">
        <InputGroup style={{ maxWidth: '400px' }}>
          <Form.Control
            placeholder="Buscar por nombre del pasajero..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
          <Button variant="dark" type="submit" disabled={searching}>
            {searching ? <Spinner size="sm" animation="border" /> : <i className="bi bi-search" />}
          </Button>
          {search && (
            <Button variant="outline-secondary" onClick={() => { setSearch(''); load(); }}>
              <i className="bi bi-x" />
            </Button>
          )}
        </InputGroup>
      </Form>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : (
        <div className="bg-white rounded shadow-sm overflow-hidden">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>#Reservación</th>
                <th>Pasajero</th>
                <th>Vuelo</th>
                <th>Fecha</th>
                <th>Pago</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {reservations.length === 0 ? (
                <tr><td colSpan={6} className="text-center text-muted py-4">Sin resultados.</td></tr>
              ) : reservations.map(r => (
                <tr key={r.cod_Reservacion}>
                  <td className="fw-bold">{r.cod_Reservacion}</td>
                  <td>{r.userName ?? `Usuario #${r.id_Usuario}`}</td>
                  <td>{r.flightNumber ?? `#${r.num_Vuelo}`}</td>
                  <td className="small">{new Date(r.fecha).toLocaleDateString('es-CR')}</td>
                  <td>
                    <Badge bg={PAYMENT_BADGE[r.estado_Pago] ?? 'secondary'}>{r.estado_Pago}</Badge>
                  </td>
                  <td className="d-flex gap-2">
                    <Button
                      variant="outline-dark"
                      size="sm"
                      onClick={() => navigate(`/aeropuerto/checkin?cod=${r.cod_Reservacion}`)}
                      title="Realizar Check-in"
                    >
                      <i className="bi bi-check2-square me-1" />Check-in
                    </Button>
                    <Button
                      variant="outline-secondary"
                      size="sm"
                      onClick={() => openBaggage(r.cod_Reservacion, r.userName)}
                      title="Ver maletas"
                    >
                      <i className="bi bi-luggage me-1" />Maletas
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}

      <Modal show={!!baggageModal} onHide={() => setBaggageModal(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">
            Maletas — {baggageModal?.userName ?? baggageModal?.cod}
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {baggageLoading ? (
            <div className="text-center py-3"><Spinner animation="border" /></div>
          ) : baggages.length === 0 ? (
            <p className="text-muted text-center mb-0">Sin maletas registradas.</p>
          ) : (
            <>
              <Table size="sm" className="mb-2">
                <thead><tr><th>#</th><th>Color</th><th>Peso (kg)</th></tr></thead>
                <tbody>
                  {baggages.map(b => (
                    <tr key={b.num_Maleta}>
                      <td>{b.num_Maleta}</td>
                      <td>{b.color}</td>
                      <td>{b.peso}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
              <Card className={`border-0 ${baggageFee(baggages.length) > 0 ? 'bg-warning bg-opacity-10' : 'bg-success bg-opacity-10'}`}>
                <Card.Body className="py-2 px-3 small">
                  {baggages.length === 1
                    ? '1ra maleta: gratis'
                    : `Cargo adicional: $${baggageFee(baggages.length)} (${baggages.length} maletas)`}
                </Card.Body>
              </Card>
            </>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" size="sm" onClick={() => setBaggageModal(null)}>Cerrar</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ReservationManagementView;

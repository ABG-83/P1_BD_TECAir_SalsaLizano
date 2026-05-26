import { useState } from 'react';
import { Row, Col, Form, Button, Alert, Card, Table } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { checkInService } from '../../services/checkInService';
import type { BoardingPass, Baggage, BaggageCreate } from '../../types';

const CheckInManagementView = () => {
  const [searchParams] = useSearchParams();
  const [codReservacion, setCodReservacion] = useState(searchParams.get('cod') ?? '');
  const [apellidos, setApellidos] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [boardingPass, setBoardingPass] = useState<BoardingPass | null>(null);
  const [baggages, setBaggages] = useState<Baggage[]>([]);
  const [addingBag, setAddingBag] = useState(false);
  const [bagForm, setBagForm] = useState<{ peso: string; color: string }>({ peso: '', color: '' });

  const handleCheckIn = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setError('');
    setBoardingPass(null);
    setBaggages([]);
    setLoading(true);
    try {
      const pass = await checkInService.doCheckIn(codReservacion.trim(), apellidos);
      setBoardingPass(pass);
      const bags = await checkInService.getBaggageByReservation(codReservacion.trim()).catch(() => []);
      setBaggages(bags);
    } catch {
      setError('Reservación no encontrada. Verifica el código y apellidos.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddBag = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    if (!boardingPass) return;
    setAddingBag(true);
    try {
      const dto: BaggageCreate = {
        peso: Number(bagForm.peso),
        color: bagForm.color,
        cod_Reservacion: boardingPass.cod_Reservacion,
        id_Usuario: 0,
      };
      const bag = await checkInService.addBaggage(dto);
      setBaggages(prev => [...prev, bag]);
      setBagForm({ peso: '', color: '' });
    } catch {
      setError('No se pudo agregar la maleta.');
    } finally {
      setAddingBag(false);
    }
  };

  const baggageFee = () => {
    if (baggages.length <= 1) return 0;
    if (baggages.length === 2) return 50;
    return 50 + (baggages.length - 2) * 75;
  };

  const reset = () => {
    setBoardingPass(null);
    setBaggages([]);
    setCodReservacion('');
    setApellidos('');
    setError('');
  };

  return (
    <>
      <h3 className="fw-bold mb-1">Check-in</h3>
      <p className="text-muted mb-4">Gestión de chequeo de pasajeros en el aeropuerto.</p>

      {!boardingPass ? (
        <Row className="justify-content-center">
          <Col md={6}>
            <div className="bg-white rounded shadow-sm p-4">
              <h5 className="fw-bold mb-4">Buscar Reservación</h5>
              {error && <Alert variant="danger">{error}</Alert>}
              <Form onSubmit={handleCheckIn}>
                <Form.Group className="mb-3">
                  <Form.Label className="text-muted small text-uppercase fw-bold">Código de Reservación</Form.Label>
                  <Form.Control type="text" className="minimal-input" placeholder="Ej: RES-001" value={codReservacion} onChange={e => setCodReservacion(e.target.value)} required />
                </Form.Group>
                <Form.Group className="mb-4">
                  <Form.Label className="text-muted small text-uppercase fw-bold">Apellidos del Pasajero</Form.Label>
                  <Form.Control type="text" className="minimal-input" placeholder="Apellidos" value={apellidos} onChange={e => setApellidos(e.target.value)} required />
                </Form.Group>
                <Button variant="dark" type="submit" className="w-100" disabled={loading}>
                  {loading ? 'Procesando...' : 'Realizar Check-in'}
                </Button>
              </Form>
            </div>
          </Col>
        </Row>
      ) : (
        <Row className="g-4">
          <Col md={5}>
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-4">
                <div className="d-flex justify-content-between align-items-start mb-3">
                  <h5 className="fw-bold mb-0">Pase de Abordar</h5>
                  <span className="badge bg-success">Check-in OK</span>
                </div>
                {[
                  ['Vuelo',       `#${boardingPass.num_Vuelo}`],
                  ['Reservación', `#${boardingPass.cod_Reservacion}`],
                  ['Asiento',     boardingPass.asiento],
                  ['Puerta',      boardingPass.puerta_Abordaje],
                  ['Impreso',     new Date(boardingPass.hora_Impresion).toLocaleString('es-CR')],
                ].map(([label, val]) => (
                  <div key={label} className="d-flex justify-content-between mb-2">
                    <span className="text-muted">{label}</span>
                    <span className="fw-bold">{val}</span>
                  </div>
                ))}
                <Button variant="outline-secondary" size="sm" className="w-100 mt-3" onClick={reset}>
                  Nuevo Check-in
                </Button>
              </Card.Body>
            </Card>
          </Col>

          <Col md={7}>
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-4">
                <h5 className="fw-bold mb-3">Maletas</h5>
                {baggages.length > 0 && (
                  <Table size="sm" className="mb-3">
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
                )}
                {baggages.length > 0 && (
                  <Alert variant={baggageFee() > 0 ? 'warning' : 'success'} className="py-2 small">
                    {baggages.length === 1
                      ? '1ra maleta: gratis'
                      : `Cargo adicional: $${baggageFee()} (${baggages.length} maletas)`}
                  </Alert>
                )}
                <Form onSubmit={handleAddBag}>
                  <Row className="g-2 align-items-end">
                    <Col>
                      <Form.Label className="text-muted small text-uppercase fw-bold">Color</Form.Label>
                      <Form.Control size="sm" className="minimal-input" placeholder="Ej: Negro" value={bagForm.color} onChange={e => setBagForm(p => ({ ...p, color: e.target.value }))} required />
                    </Col>
                    <Col>
                      <Form.Label className="text-muted small text-uppercase fw-bold">Peso (kg)</Form.Label>
                      <Form.Control size="sm" type="number" step="0.1" min="0.1" className="minimal-input" placeholder="23.0" value={bagForm.peso} onChange={e => setBagForm(p => ({ ...p, peso: e.target.value }))} required />
                    </Col>
                    <Col xs="auto">
                      <Button variant="dark" size="sm" type="submit" disabled={addingBag}>
                        {addingBag ? '...' : 'Agregar'}
                      </Button>
                    </Col>
                  </Row>
                </Form>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}
    </>
  );
};

export default CheckInManagementView;

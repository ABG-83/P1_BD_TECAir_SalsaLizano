import { useState } from 'react';
import { Container, Form, Button, Alert, Card } from 'react-bootstrap';
import { checkInService } from '../services/checkInService';
import type { BoardingPass } from '../types';

const CheckInView = () => {
  const [codReservacion, setCodReservacion] = useState('');
  const [apellidos, setApellidos] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [boardingPass, setBoardingPass] = useState<BoardingPass | null>(null);

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setError('');
    setBoardingPass(null);
    setLoading(true);
    try {
      const pass = await checkInService.doCheckIn(Number(codReservacion), apellidos);
      setBoardingPass(pass);
    } catch {
      setError('No se encontró la reservación. Verifica el código y los apellidos.');
    } finally {
      setLoading(false);
    }
  };

  if (boardingPass) {
    return (
      <Container className="py-5 d-flex justify-content-center">
        <div className="w-100" style={{ maxWidth: '500px' }}>
          <Alert variant="success" className="text-center fw-bold">
            ¡Check-in realizado con éxito!
          </Alert>
          <Card className="border-0 shadow-sm">
            <Card.Body className="p-4">
              <h4 className="fw-bold mb-4 text-center">Pase de Abordar</h4>
              <div className="d-flex justify-content-between mb-3">
                <span className="text-muted">Vuelo</span>
                <span className="fw-bold">#{boardingPass.num_Vuelo}</span>
              </div>
              <div className="d-flex justify-content-between mb-3">
                <span className="text-muted">Reservación</span>
                <span className="fw-bold">#{boardingPass.cod_Reservacion}</span>
              </div>
              <div className="d-flex justify-content-between mb-3">
                <span className="text-muted">Asiento</span>
                <span className="fw-bold fs-4">{boardingPass.asiento}</span>
              </div>
              <div className="d-flex justify-content-between mb-3">
                <span className="text-muted">Puerta</span>
                <span className="fw-bold fs-4">{boardingPass.puerta_Abordaje}</span>
              </div>
              <div className="d-flex justify-content-between">
                <span className="text-muted">Impreso</span>
                <span className="fw-bold">{new Date(boardingPass.hora_Impresion).toLocaleString('es-CR')}</span>
              </div>
            </Card.Body>
          </Card>
          <Button variant="outline-dark" className="w-100 mt-4" onClick={() => { setBoardingPass(null); setCodReservacion(''); setApellidos(''); }}>
            Nuevo Check-in
          </Button>
        </div>
      </Container>
    );
  }

  return (
    <Container className="py-5 d-flex justify-content-center">
      <div className="bg-white p-5 rounded shadow-sm border-0 w-100" style={{ maxWidth: '500px' }}>
        <h3 className="mb-4 fw-bold text-center">Pre-Chequeo</h3>
        <p className="text-muted text-center mb-4">Ingresa tus datos para obtener tu pase de abordar.</p>

        {error && <Alert variant="danger" className="py-2">{error}</Alert>}

        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-4" controlId="reservationCode">
            <Form.Label className="text-muted small text-uppercase fw-bold">Código de Reservación</Form.Label>
            <Form.Control
              type="number"
              className="minimal-input"
              placeholder="Ej: 1001"
              value={codReservacion}
              onChange={e => setCodReservacion(e.target.value)}
              required
            />
          </Form.Group>

          <Form.Group className="mb-4" controlId="lastName">
            <Form.Label className="text-muted small text-uppercase fw-bold">Apellidos</Form.Label>
            <Form.Control
              type="text"
              className="minimal-input"
              placeholder="Tal como aparecen en la reserva"
              value={apellidos}
              onChange={e => setApellidos(e.target.value)}
              required
            />
          </Form.Group>

          <Button variant="dark" type="submit" className="w-100 mt-2 py-2" disabled={loading}>
            {loading ? 'Procesando...' : 'Realizar Check-In'}
          </Button>
        </Form>
      </div>
    </Container>
  );
};

export default CheckInView;

import { useState } from 'react';
import { Container, Row, Col, Form, Button, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useAirport } from '../hooks/useAirports';

const HomeView = () => {
  const navigate = useNavigate();
  const { airports, loading, error } = useAirport();

  const [origen, setOrigen] = useState('');
  const [destino, setDestino] = useState('');
  const [fecha, setFecha] = useState('');

  const handleSearch = (e: { preventDefault(): void }) => {
    e.preventDefault();
    navigate(`/reservacion?origen=${origen}&destino=${destino}&fecha=${fecha}`);
  };

  return (
    <Container className="py-4">
      <div className="p-4 mb-5 bg-accent-soft text-dark rounded shadow-sm">
        <h5 className="text-accent fw-bold mb-2">¡Ofertas Especiales!</h5>
        <p className="mb-0 text-muted">
          Aprovecha nuestros descuentos exclusivos para estudiantes en rutas seleccionadas.{' '}
          <a href="/promociones" className="text-dark fw-bold">Ver promociones →</a>
        </p>
      </div>

      <div className="bg-white p-5 rounded shadow-sm border-0">
        <h3 className="mb-4 fw-bold">Encuentra tu próximo vuelo</h3>

        {error && (
          <Alert variant="danger" className="mb-4">
            {error}
          </Alert>
        )}

        <Form onSubmit={handleSearch}>
          <Row className="mb-4">
            <Form.Group as={Col} md="6" controlId="origin" className="mb-4 mb-md-0">
              <Form.Label className="text-muted small text-uppercase fw-bold">Origen</Form.Label>
              <Form.Select
                required
                className="minimal-input fs-5"
                value={origen}
                onChange={e => setOrigen(e.target.value)}
                disabled={loading}
              >
                <option key="origin-placeholder" value="">{loading ? 'Cargando aeropuertos...' : '¿Desde dónde viajas?'}</option>
                {!loading && airports.map(a => (
                  <option key={`origin-${a.airportId}`} value={a.airportId}>{a.name}</option>
                ))}
              </Form.Select>
            </Form.Group>

            <Form.Group as={Col} md="6" controlId="destination">
              <Form.Label className="text-muted small text-uppercase fw-bold">Destino</Form.Label>
              <Form.Select
                required
                className="minimal-input fs-5"
                value={destino}
                onChange={e => setDestino(e.target.value)}
                disabled={loading}
              >
                <option key="destination-placeholder" value="">{loading ? 'Cargando aeropuertos...' : '¿A dónde vas?'}</option>
                {!loading && airports.filter(a => a.airportId !== Number(origen)).map(a => (
                  <option key={`destination-${a.airportId}`} value={a.airportId}>{a.name}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Row>

          <Row className="mb-4">
            <Form.Group as={Col} md="6" controlId="date">
              <Form.Label className="text-muted small text-uppercase fw-bold">Fecha de Salida</Form.Label>
              <Form.Control
                type="date"
                required
                className="minimal-input fs-5"
                min={new Date().toISOString().split('T')[0]}
                value={fecha}
                onChange={e => setFecha(e.target.value)}
              />
            </Form.Group>
          </Row>

          <div className="text-end mt-4">
            <Button variant="dark" type="submit" size="lg" className="px-5">
              Buscar Vuelos
            </Button>
          </div>
        </Form>
      </div>
    </Container>
  );
};

export default HomeView;

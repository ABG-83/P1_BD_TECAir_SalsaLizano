import { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { airportService } from '../services/airportService';
import type { Airport } from '../types';

const FALLBACK_AIRPORTS: Airport[] = [
  { id_Aeropuerto: 1, nombre: 'Aeropuerto Internacional Juan Santamaría', ubicacion: 'Alajuela, Costa Rica' },
  { id_Aeropuerto: 2, nombre: 'Aeropuerto Internacional Daniel Oduber', ubicacion: 'Liberia, Costa Rica' },
  { id_Aeropuerto: 3, nombre: 'Aeropuerto Internacional El Dorado', ubicacion: 'Bogotá, Colombia' },
  { id_Aeropuerto: 4, nombre: 'Aeropuerto Internacional Jorge Chávez', ubicacion: 'Lima, Perú' },
  { id_Aeropuerto: 5, nombre: 'Aeropuerto Internacional Tocumen', ubicacion: 'Ciudad de Panamá, Panamá' },
  { id_Aeropuerto: 6, nombre: 'Aeropuerto Internacional Benito Juárez', ubicacion: 'Ciudad de México, México' },
];

const HomeView = () => {
  const navigate = useNavigate();
  const [airports, setAirports] = useState<Airport[]>([]);
  const [origen, setOrigen] = useState('');
  const [destino, setDestino] = useState('');
  const [fecha, setFecha] = useState('');

  useEffect(() => {
    airportService.getAll()
      .then(setAirports)
      .catch(() => setAirports(FALLBACK_AIRPORTS));
  }, []);

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
        <Form onSubmit={handleSearch}>
          <Row className="mb-4">
            <Form.Group as={Col} md="6" controlId="origin" className="mb-4 mb-md-0">
              <Form.Label className="text-muted small text-uppercase fw-bold">Origen</Form.Label>
              <Form.Select required className="minimal-input fs-5" value={origen} onChange={e => setOrigen(e.target.value)}>
                <option value="">¿Desde dónde viajas?</option>
                {airports.map(a => (
                  <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>
                ))}
              </Form.Select>
            </Form.Group>

            <Form.Group as={Col} md="6" controlId="destination">
              <Form.Label className="text-muted small text-uppercase fw-bold">Destino</Form.Label>
              <Form.Select required className="minimal-input fs-5" value={destino} onChange={e => setDestino(e.target.value)}>
                <option value="">¿A dónde vas?</option>
                {airports.filter(a => String(a.id_Aeropuerto) !== origen).map(a => (
                  <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>
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

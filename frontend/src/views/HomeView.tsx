import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';

const HomeView = () => {
  const navigate = useNavigate();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    navigate('/reservacion');
  };

  return (
    <Container className="py-4">
      {/* Banner de Promociones */}
      <div className="p-4 mb-5 bg-accent-soft text-dark rounded shadow-sm">
        <h5 className="text-accent fw-bold mb-2">¡Ofertas Especiales!</h5>
        <p className="mb-0 text-muted">Aprovecha nuestros descuentos exclusivos para estudiantes en rutas seleccionadas.</p>
      </div>

      <div className="bg-white p-5 rounded shadow-sm border-0">
        <h3 className="mb-4 fw-bold">Encuentra tu próximo vuelo</h3>
        <Form onSubmit={handleSearch}>
          <Row className="mb-4">
            <Form.Group as={Col} md="6" controlId="origin" className="mb-4 mb-md-0">
              <Form.Label className="text-muted small text-uppercase fw-bold">Origen</Form.Label>
              <Form.Select required className="minimal-input fs-5">
                <option value="">¿Desde dónde viajas?</option>
                <option value="SJO">San José (SJO)</option>
                <option value="LIR">Liberia (LIR)</option>
              </Form.Select>
            </Form.Group>

            <Form.Group as={Col} md="6" controlId="destination">
              <Form.Label className="text-muted small text-uppercase fw-bold">Destino</Form.Label>
              <Form.Select required className="minimal-input fs-5">
                <option value="">¿A dónde vas?</option>
                <option value="MIA">Miami (MIA)</option>
                <option value="MAD">Madrid (MAD)</option>
              </Form.Select>
            </Form.Group>
          </Row>
          
          <Row className="mb-4">
             <Form.Group as={Col} md="6" controlId="date">
              <Form.Label className="text-muted small text-uppercase fw-bold">Fecha de Salida</Form.Label>
              <Form.Control type="date" required className="minimal-input fs-5" />
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

import { Container, Form, Button } from 'react-bootstrap';

const CheckInView = () => {
  const handleCheckIn = (e: React.FormEvent) => {
    e.preventDefault();
    alert("Pre-chequeo realizado con éxito");
  };

  return (
    <Container className="py-5 d-flex justify-content-center">
      <div className="bg-white p-5 rounded shadow-sm border-0 w-100" style={{ maxWidth: '500px' }}>
        <h3 className="mb-4 fw-bold text-center">Pre-Chequeo</h3>
        <p className="text-muted text-center mb-4">Ingresa tus datos para obtener tu pase de abordar.</p>
        
        <Form onSubmit={handleCheckIn}>
          <Form.Group className="mb-4" controlId="reservationCode">
            <Form.Label className="text-muted small text-uppercase fw-bold">Código de Reservación</Form.Label>
            <Form.Control type="text" className="minimal-input" placeholder="Ej: ABC123" required />
          </Form.Group>
          
          <Form.Group className="mb-4" controlId="lastName">
            <Form.Label className="text-muted small text-uppercase fw-bold">Apellidos</Form.Label>
            <Form.Control type="text" className="minimal-input" placeholder="Tal como aparecen en la reserva" required />
          </Form.Group>

          <Button variant="dark" type="submit" className="w-100 mt-2 py-2">
            Realizar Check-In
          </Button>
        </Form>
      </div>
    </Container>
  );
};

export default CheckInView;

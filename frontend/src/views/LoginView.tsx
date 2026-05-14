import { Container, Form, Button } from 'react-bootstrap';

const LoginView = () => {
  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    alert("Iniciando sesión...");
  };

  return (
    <Container className="py-5 d-flex justify-content-center align-items-center" style={{ minHeight: '70vh' }}>
      <div className="bg-white p-5 rounded shadow-sm border-0 w-100" style={{ maxWidth: '450px' }}>
        <h3 className="text-center fw-bold mb-4">Bienvenido</h3>
        <Form onSubmit={handleLogin}>
          <Form.Group className="mb-4" controlId="email">
            <Form.Label className="text-muted small text-uppercase fw-bold">Correo Electrónico</Form.Label>
            <Form.Control type="email" className="minimal-input" placeholder="ejemplo@correo.com" required />
          </Form.Group>

          <Form.Group className="mb-4" controlId="password">
            <Form.Label className="text-muted small text-uppercase fw-bold">Contraseña</Form.Label>
            <Form.Control type="password" className="minimal-input" placeholder="Tu contraseña" required />
          </Form.Group>

          <Button variant="dark" type="submit" className="w-100 py-2 mt-3">
            Ingresar
          </Button>
        </Form>
        <div className="text-center mt-4 pt-3 border-top">
          <span className="text-muted">¿No tienes cuenta?</span> <a href="#" className="text-dark fw-bold text-decoration-none">Regístrate aquí</a>
        </div>
      </div>
    </Container>
  );
};

export default LoginView;

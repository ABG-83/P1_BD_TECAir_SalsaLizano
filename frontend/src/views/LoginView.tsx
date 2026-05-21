import { useState } from 'react';
import { Container, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { authService } from '../services/authService';

const LoginView = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [correo, setCorreo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const user = await authService.login({ correo, contrasena });
      login(user);
      if (user.rol === 'funcionario' || user.rol === 'administrador') {
        navigate('/aeropuerto');
      } else {
        navigate('/');
      }
    } catch {
      setError('Correo o contraseña incorrectos.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5 d-flex justify-content-center align-items-center" style={{ minHeight: '70vh' }}>
      <div className="bg-white p-5 rounded shadow-sm border-0 w-100" style={{ maxWidth: '450px' }}>
        <h3 className="text-center fw-bold mb-4">Bienvenido</h3>
        {error && <Alert variant="danger" className="py-2">{error}</Alert>}
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-4" controlId="email">
            <Form.Label className="text-muted small text-uppercase fw-bold">Correo Electrónico</Form.Label>
            <Form.Control
              type="email"
              className="minimal-input"
              placeholder="ejemplo@correo.com"
              value={correo}
              onChange={e => setCorreo(e.target.value)}
              required
            />
          </Form.Group>

          <Form.Group className="mb-4" controlId="password">
            <Form.Label className="text-muted small text-uppercase fw-bold">Contraseña</Form.Label>
            <Form.Control
              type="password"
              className="minimal-input"
              placeholder="Tu contraseña"
              value={contrasena}
              onChange={e => setContrasena(e.target.value)}
              required
            />
          </Form.Group>

          <Button variant="dark" type="submit" className="w-100 py-2 mt-3" disabled={loading}>
            {loading ? 'Ingresando...' : 'Ingresar'}
          </Button>
        </Form>
        <div className="text-center mt-4 pt-3 border-top">
          <span className="text-muted">¿No tienes cuenta?</span>{' '}
          <Link to="/registro" className="text-dark fw-bold text-decoration-none">Regístrate aquí</Link>
        </div>
      </div>
    </Container>
  );
};

export default LoginView;

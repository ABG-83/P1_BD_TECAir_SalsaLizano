import { useState } from 'react';
import { Container, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { userService } from '../services/userService';

const RegisterView = () => {
  const navigate = useNavigate();
  const [isStudent, setIsStudent] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({
    nombre: '',
    telefono: '',
    correo: '',
    contrasena: '',
    universidad: '',
    carnet: '',
  });

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await userService.create({
        nombre: form.nombre,
        correo: form.correo,
        telefono: form.telefono,
        rol: 'cliente',
        millas: 0,
        contrasena: form.contrasena,
        ...(isStudent && { carnet: form.carnet, universidad: form.universidad }),
      });
      navigate('/login');
    } catch {
      setError('No se pudo registrar el usuario. Verifica que el correo no esté en uso.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5 d-flex justify-content-center align-items-center" style={{ minHeight: '80vh' }}>
      <div className="bg-white p-5 rounded shadow-sm border-0 w-100" style={{ maxWidth: '600px' }}>
        <h3 className="text-center fw-bold mb-4">Crear una Cuenta</h3>
        {error && <Alert variant="danger" className="py-2">{error}</Alert>}
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-4" controlId="fullName">
            <Form.Label className="text-muted small text-uppercase fw-bold">Nombre Completo</Form.Label>
            <Form.Control type="text" className="minimal-input" placeholder="Tu nombre y apellidos" value={form.nombre} onChange={set('nombre')} required />
          </Form.Group>

          <Form.Group className="mb-4" controlId="phone">
            <Form.Label className="text-muted small text-uppercase fw-bold">Teléfono</Form.Label>
            <Form.Control type="tel" className="minimal-input" placeholder="+506 8888 8888" value={form.telefono} onChange={set('telefono')} required />
          </Form.Group>

          <Form.Group className="mb-4" controlId="email">
            <Form.Label className="text-muted small text-uppercase fw-bold">Correo Electrónico</Form.Label>
            <Form.Control type="email" className="minimal-input" placeholder="ejemplo@correo.com" value={form.correo} onChange={set('correo')} required />
          </Form.Group>

          <Form.Group className="mb-4" controlId="password">
            <Form.Label className="text-muted small text-uppercase fw-bold">Contraseña</Form.Label>
            <Form.Control type="password" className="minimal-input" placeholder="Crea una contraseña segura" value={form.contrasena} onChange={set('contrasena')} required />
          </Form.Group>

          <Form.Group className="mb-4" controlId="isStudent">
            <Form.Check
              type="checkbox"
              label="Soy estudiante universitario"
              className="fw-medium text-dark"
              onChange={e => setIsStudent(e.target.checked)}
            />
          </Form.Group>

          {isStudent && (
            <div className="bg-light p-4 rounded mb-4 border">
              <h6 className="fw-bold mb-3">Información de Estudiante (Programa de Millas)</h6>
              <Form.Group className="mb-3" controlId="university">
                <Form.Label className="text-muted small text-uppercase fw-bold">Universidad</Form.Label>
                <Form.Control type="text" className="minimal-input bg-white" placeholder="Ej: TEC, UCR, UNA..." value={form.universidad} onChange={set('universidad')} required />
              </Form.Group>
              <Form.Group className="mb-2" controlId="carnet">
                <Form.Label className="text-muted small text-uppercase fw-bold">Carné Estudiantil</Form.Label>
                <Form.Control type="text" className="minimal-input bg-white" placeholder="Ej: 2026123456" value={form.carnet} onChange={set('carnet')} required />
              </Form.Group>
            </div>
          )}

          <Button variant="dark" type="submit" className="w-100 py-3 mt-2 fs-5 fw-bold" disabled={loading}>
            {loading ? 'Registrando...' : 'Registrarme'}
          </Button>
        </Form>
        <div className="text-center mt-4 pt-3 border-top">
          <span className="text-muted">¿Ya tienes cuenta?</span>{' '}
          <Link to="/login" className="text-dark fw-bold text-decoration-none">Inicia sesión aquí</Link>
        </div>
      </div>
    </Container>
  );
};

export default RegisterView;

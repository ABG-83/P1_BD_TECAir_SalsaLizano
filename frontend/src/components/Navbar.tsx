import { Navbar, Container, Nav, Button, NavDropdown } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const AppNavbar = () => {
  const { user, isAuthenticated, isStaff, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <Navbar bg="white" variant="light" expand="lg" className="mb-5 py-3 shadow-sm">
      <Container>
        <Navbar.Brand as={Link as any} to="/" className="fs-1 text-decoration-none">
          <span style={{ color: '#004A99', fontWeight: 'bold' }}>TEC</span>
          <span className="text-dark fw-light">Air</span>
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" className="border-0" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto ms-4">
            <Nav.Link as={Link as any} to="/" className="text-dark fw-medium mx-2">Búsqueda de Vuelos</Nav.Link>
            <Nav.Link as={Link as any} to="/promociones" className="text-dark fw-medium mx-2">Promociones</Nav.Link>
            <Nav.Link as={Link as any} to="/checkin" className="text-dark fw-medium mx-2">Pre-Chequeo</Nav.Link>
          </Nav>
          <Nav className="align-items-center gap-2">
            {isAuthenticated ? (
              <>
                {isStaff && (
                  <Button as={Link as any} to="/aeropuerto" variant="outline-primary" size="sm" className="px-3">
                    Panel Aeropuerto
                  </Button>
                )}
                <NavDropdown
                  title={<span className="fw-medium">{user?.nombre.split(' ')[0]}</span>}
                  align="end"
                  id="user-dropdown"
                >
                  {!isStaff && (
                    <NavDropdown.Item as={Link as any} to="/mis-reservaciones">
                      <i className="bi bi-ticket me-2"></i>Mis Reservaciones
                    </NavDropdown.Item>
                  )}
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={handleLogout} className="text-danger">
                    <i className="bi bi-box-arrow-right me-2"></i>Cerrar sesión
                  </NavDropdown.Item>
                </NavDropdown>
              </>
            ) : (
              <Button as={Link as any} to="/login" variant="outline-dark" className="px-4">
                Iniciar Sesión
              </Button>
            )}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default AppNavbar;

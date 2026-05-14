import { Navbar, Container, Nav, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';

const AppNavbar = () => {
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
          <Nav>
            <Button as={Link as any} to="/login" variant="outline-dark" className="px-4">Iniciar Sesión</Button>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default AppNavbar;

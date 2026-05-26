import { useEffect } from 'react';
import { Container, Row, Col, Card, Button, Spinner, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { usePromotions } from '../hooks/usePromotions';

const PLACEHOLDER_IMG = 'https://images.unsplash.com/photo-1436491865332-7a61a109cc05?w=800&q=80';

const PromotionsView = () => {
  const navigate = useNavigate();
  const { promotions, loading, error, getActivePromotions } = usePromotions();

  useEffect(() => {
    getActivePromotions();
  }, [getActivePromotions]);

  const formatDateOnly = (dateString: string | undefined): string => {
    if (!dateString) return '—';
    const parts = dateString.split('T')[0].split('-');
    if (parts.length !== 3) return new Date(dateString).toLocaleDateString('es-CR');
    const [year, month, day] = parts;
    return `${day}/${month}/${year}`;
  };

  return (
    <Container className="py-5">
      <div className="mb-5 text-center">
        <h2 className="fw-bold mb-3">Promociones Exclusivas</h2>
        <p className="text-muted fs-5">
          Descubre las mejores ofertas para tus próximos viajes. Si eres estudiante, acumulas millas dobles.
        </p>
      </div>

      {error && <Alert variant="danger" className="text-center">{error}</Alert>}

      {loading ? (
        <div className="text-center py-5">
          <Spinner animation="border" variant="dark" />
          <p className="text-muted mt-2 small">Sincronizando con TECAir...</p>
        </div>
      ) : promotions.length === 0 && !error ? (
        <p className="text-center text-muted fs-5">No hay promociones activas en este momento.</p>
      ) : (
        <Row>
          {promotions.map(promo => {
            // Your view properties are now standard Spanish snake_case again!
            const origen = promo.aeropuertoOrigen?.nombre ?? `Aeropuerto #${promo.id_Aeropuerto_Origen}`;
            const destino = promo.aeropuertoDestino?.nombre ?? `Aeropuerto #${promo.id_Aeropuerto_Destino}`;
            const periodo = `${formatDateOnly(promo.fecha_Inicio)} — ${formatDateOnly(promo.fecha_Fin)}`;

            return (
              <Col md={6} lg={4} key={promo.id_Promocion} className="mb-4">
                <Card className="border-0 shadow-sm h-100 overflow-hidden">
                  <div
                    style={{
                      height: '200px',
                      backgroundImage: `url(${promo.imagen || PLACEHOLDER_IMG})`,
                      backgroundSize: 'cover',
                      backgroundPosition: 'center',
                    }}
                  />
                  <Card.Body className="p-4 d-flex flex-column">
                    <div className="text-accent fw-bold small mb-2">{periodo}</div>
                    <Card.Title className="fw-bold fs-4 mb-3">
                      {origen}<br /><span className="text-muted fs-5">a</span> {destino}
                    </Card.Title>
                    <div className="mt-auto d-flex justify-content-between align-items-center">
                      <span className="fs-3 fw-bold text-dark">${promo.precio.toFixed(2)}</span>
                      <Button variant="outline-dark" className="px-3" onClick={() => navigate('/')}>
                        Ver Vuelos
                      </Button>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
            );
          })}
        </Row>
      )}
    </Container>
  );
};

export default PromotionsView;

import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { promotionService } from '../services/promotionService';
import type { Promotion } from '../types';

const PLACEHOLDER_IMG = 'https://images.unsplash.com/photo-1436491865332-7a61a109cc05?w=800&q=80';

const PromotionsView = () => {
  const navigate = useNavigate();
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    promotionService.getActive()
      .then(setPromotions)
      .catch(() => setPromotions([]))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container className="py-5">
      <div className="mb-5 text-center">
        <h2 className="fw-bold mb-3">Promociones Exclusivas</h2>
        <p className="text-muted fs-5">Descubre las mejores ofertas para tus próximos viajes. Si eres estudiante, acumulas millas dobles.</p>
      </div>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : promotions.length === 0 ? (
        <p className="text-center text-muted fs-5">No hay promociones activas en este momento.</p>
      ) : (
        <Row>
          {promotions.map(promo => {
            const origen  = promo.aeropuertoOrigen?.nombre  ?? `Aeropuerto #${promo.id_Aeropuerto_Origen}`;
            const destino = promo.aeropuertoDestino?.nombre ?? `Aeropuerto #${promo.id_Aeropuerto_Destino}`;
            const periodo = `${new Date(promo.fecha_Inicio).toLocaleDateString('es-CR')} — ${new Date(promo.fecha_Fin).toLocaleDateString('es-CR')}`;

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

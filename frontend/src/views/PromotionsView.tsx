import { Container, Row, Col, Card, Button } from 'react-bootstrap';

const promotions = [
  {
    id: 1,
    origin: "San José (SJO)",
    destination: "Madrid (MAD)",
    price: 399.00,
    period: "Agosto 2026 - Noviembre 2026",
    image: "https://images.unsplash.com/photo-1539037116277-4db20202d03e?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
    description: "Vuelo directo con maleta de 23kg incluida."
  },
  {
    id: 2,
    origin: "Liberia (LIR)",
    destination: "Miami (MIA)",
    price: 125.00,
    period: "Septiembre 2026 - Diciembre 2026",
    image: "https://images.unsplash.com/photo-1514214246283-d427a95c5d2f?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
    description: "Escapada de fin de semana con tarifa light."
  }
];

const PromotionsView = () => {
  return (
    <Container className="py-5">
      <div className="mb-5 text-center">
        <h2 className="fw-bold mb-3">Promociones Exclusivas</h2>
        <p className="text-muted fs-5">Descubre las mejores ofertas para tus próximos viajes. Si eres estudiante, acumulas millas dobles.</p>
      </div>

      <Row>
        {promotions.map((promo) => (
          <Col md={6} lg={4} key={promo.id} className="mb-4">
            <Card className="border-0 shadow-sm h-100 overflow-hidden">
              <div 
                style={{ 
                  height: '200px', 
                  backgroundImage: `url(${promo.image})`,
                  backgroundSize: 'cover',
                  backgroundPosition: 'center'
                }}
              />
              <Card.Body className="p-4 d-flex flex-column">
                <div className="text-accent fw-bold small mb-2">{promo.period}</div>
                <Card.Title className="fw-bold fs-4 mb-3">
                  {promo.origin} <br/><span className="text-muted fs-5">a</span> {promo.destination}
                </Card.Title>
                <Card.Text className="text-muted mb-4">
                  {promo.description}
                </Card.Text>
                <div className="mt-auto d-flex justify-content-between align-items-center">
                  <span className="fs-3 fw-bold text-dark">${promo.price}</span>
                  <Button variant="outline-dark" className="px-3">Ver Vuelos</Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </Container>
  );
};

export default PromotionsView;

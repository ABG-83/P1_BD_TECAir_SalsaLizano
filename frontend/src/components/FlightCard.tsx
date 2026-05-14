import { Card, Button } from 'react-bootstrap';

interface FlightCardProps {
  flight: {
    id: string;
    origin: string;
    destination: string;
    date: string;
    time: string;
    price: number;
    availableSeats: number;
  };
  onReserve: (id: string) => void;
}

const FlightCard = ({ flight, onReserve }: FlightCardProps) => {
  return (
    <Card className="mb-4 border-0 shadow-sm p-3">
      <Card.Body>
        <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center">
          <div className="mb-3 mb-md-0">
            <div className="text-muted small fw-bold mb-1">VUELO {flight.id}</div>
            <h4 className="fw-bold mb-2">
              {flight.origin} <span className="text-muted fw-normal mx-2">➔</span> {flight.destination}
            </h4>
            <div className="text-muted">
              <span className="me-3"><i className="bi bi-calendar"></i> {flight.date}</span>
              <span className="me-3"><i className="bi bi-clock"></i> {flight.time}</span>
              <span><i className="bi bi-person"></i> {flight.availableSeats} asientos libres</span>
            </div>
          </div>
          <div className="text-md-end">
            <div className="fs-3 fw-bold mb-2">${flight.price}</div>
            <Button variant="dark" className="px-4" onClick={() => onReserve(flight.id)}>
              Reservar
            </Button>
          </div>
        </div>
      </Card.Body>
    </Card>
  );
};

export default FlightCard;

import { Card, Button, Badge } from 'react-bootstrap';
import type { Flight } from '../types';

interface FlightCardProps {
  flight: Flight;
  onReserve: (id: number) => void;
}

const STATUS_LABELS: Record<string, { label: string; bg: string }> = {
  programado: { label: 'Programado', bg: 'secondary' },
  abierto:    { label: 'Abierto',    bg: 'success'   },
  cerrado:    { label: 'Cerrado',    bg: 'danger'     },
  cancelado:  { label: 'Cancelado',  bg: 'warning'    },
};

const FlightCard = ({ flight, onReserve }: FlightCardProps) => {
  const salida   = new Date(flight.hora_Salida);
  const llegada  = new Date(flight.hora_Llegada);
  const status   = STATUS_LABELS[flight.estado] ?? { label: flight.estado, bg: 'secondary' };
  const disabled = flight.estado === 'cerrado' || flight.estado === 'cancelado';

  const origen  = flight.aeropuertoOrigen?.nombre  ?? `Aeropuerto #${flight.id_Aeropuerto_Origen}`;
  const destino = flight.aeropuertoDestino?.nombre ?? `Aeropuerto #${flight.id_Aeropuerto_Destino}`;

  return (
    <Card className="mb-4 border-0 shadow-sm p-3">
      <Card.Body>
        <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center">
          <div className="mb-3 mb-md-0">
            <div className="d-flex align-items-center gap-2 mb-1">
              <span className="text-muted small fw-bold">VUELO #{flight.num_Vuelo}</span>
              <Badge bg={status.bg}>{status.label}</Badge>
            </div>
            <h4 className="fw-bold mb-2">
              {origen} <span className="text-muted fw-normal mx-2">➔</span> {destino}
            </h4>
            <div className="text-muted">
              <span className="me-3">
                <i className="bi bi-calendar me-1"></i>
                {salida.toLocaleDateString('es-CR')}
              </span>
              <span className="me-3">
                <i className="bi bi-clock me-1"></i>
                {salida.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}
                {' → '}
                {llegada.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}
              </span>
              <span className="text-muted small">
                <i className="bi bi-airplane me-1"></i>{flight.matricula}
              </span>
            </div>
          </div>
          <div className="text-md-end">
            <Button
              variant="dark"
              className="px-4"
              disabled={disabled}
              onClick={() => onReserve(flight.num_Vuelo)}
            >
              {disabled ? 'No disponible' : 'Reservar'}
            </Button>
          </div>
        </div>
      </Card.Body>
    </Card>
  );
};

export default FlightCard;

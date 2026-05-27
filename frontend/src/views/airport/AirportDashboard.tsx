import { useState, useEffect } from 'react';
import { Row, Col, Card, Spinner } from 'react-bootstrap';
import { useAuth } from '../../context/AuthContext';
import { useFlights } from '../../hooks/useFlights';
import { useUsers } from '../../hooks/useUsers';
import { useReservation } from '../../hooks/useReservation';

interface Stats {
  vuelos: number;
  usuarios: number;
  reservaciones: number;
  vuelosAbiertos: number;
}

const StatCard = ({ icon, label, value, color }: { icon: string; label: string; value: number | string; color: string }) => (
  <Card className="border-0 shadow-sm h-100">
    <Card.Body className="p-4">
      <div className="d-flex align-items-center justify-content-between">
        <div>
          <div className="text-muted small text-uppercase fw-bold mb-1">{label}</div>
          <div className="fs-2 fw-bold">{value}</div>
        </div>
        <div className={`rounded-circle d-flex align-items-center justify-content-center bg-${color} bg-opacity-10`} style={{ width: 56, height: 56 }}>
          <i className={`bi ${icon} fs-4 text-${color}`}></i>
        </div>
      </div>
    </Card.Body>
  </Card>
);

const AirportDashboard = () => {
  const { user } = useAuth();
  const { getAllFlights, loading: flightsLoading } = useFlights();
  const { getAllUsers, loading: usersLoading } = useUsers();
  const { getAllReservations, loading: resLoading } = useReservation();

  const [stats, setStats] = useState<Stats | null>(null);

  const loading = flightsLoading || usersLoading || resLoading;

  useEffect(() => {
    void (async () => {
      const [vuelos, usuarios, reservaciones] = await Promise.all([
        getAllFlights(),
        getAllUsers(),
        getAllReservations(),
      ]);
      setStats({
        vuelos: vuelos.length,
        usuarios: usuarios.length,
        reservaciones: reservaciones.length,
        vuelosAbiertos: vuelos.filter(v => v.estado === 'Boarding').length,
      });
    })();
  }, [getAllFlights, getAllUsers, getAllReservations]);

  return (
    <div>
      <h3 className="fw-bold mb-1">Bienvenido, {user?.nombre}</h3>
      <p className="text-muted mb-4">Panel de administración — {new Date().toLocaleDateString('es-CR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</p>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : (
        <Row className="g-4">
          <Col sm={6} xl={3}>
            <StatCard icon="bi-airplane" label="Total Vuelos" value={stats?.vuelos ?? 0} color="primary" />
          </Col>
          <Col sm={6} xl={3}>
            <StatCard icon="bi-airplane-fill" label="Vuelos Abiertos" value={stats?.vuelosAbiertos ?? 0} color="success" />
          </Col>
          <Col sm={6} xl={3}>
            <StatCard icon="bi-people" label="Usuarios" value={stats?.usuarios ?? 0} color="info" />
          </Col>
          <Col sm={6} xl={3}>
            <StatCard icon="bi-ticket" label="Reservaciones" value={stats?.reservaciones ?? 0} color="warning" />
          </Col>
        </Row>
      )}

      <div className="mt-5">
        <h5 className="fw-bold mb-3">Accesos Rápidos</h5>
        <Row className="g-3">
          {[
            { href: '/aeropuerto/vuelos',      icon: 'bi-airplane',      label: 'Gestionar Vuelos'      },
            { href: '/aeropuerto/checkin',      icon: 'bi-check2-square', label: 'Hacer Check-in'        },
            { href: '/aeropuerto/usuarios',     icon: 'bi-people',        label: 'Ver Usuarios'          },
            { href: '/aeropuerto/promociones',  icon: 'bi-tag',           label: 'Ver Promociones'       },
          ].map(item => (
            <Col sm={6} md={3} key={item.href}>
              <a href={item.href} className="text-decoration-none">
                <Card className="border-0 shadow-sm text-center p-3 h-100 quick-link">
                  <i className={`bi ${item.icon} fs-2 text-dark mb-2`}></i>
                  <div className="fw-medium small">{item.label}</div>
                </Card>
              </a>
            </Col>
          ))}
        </Row>
      </div>
    </div>
  );
};

export default AirportDashboard;

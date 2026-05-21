import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const NAV_ITEMS = [
  { to: '/aeropuerto',            label: 'Dashboard',   icon: 'bi-grid'           },
  { to: '/aeropuerto/vuelos',     label: 'Vuelos',      icon: 'bi-airplane'       },
  { to: '/aeropuerto/usuarios',   label: 'Usuarios',    icon: 'bi-people'         },
  { to: '/aeropuerto/checkin',    label: 'Check-in',    icon: 'bi-check2-square'  },
  { to: '/aeropuerto/promociones',label: 'Promociones', icon: 'bi-tag'            },
];

const AirportLayout = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="d-flex min-vh-100">
      {/* Sidebar */}
      <aside
        className="d-flex flex-column bg-dark text-white py-4 px-3"
        style={{ width: '240px', minWidth: '240px' }}
      >
        <div className="mb-4 px-2">
          <div className="fs-4 fw-bold">
            <span style={{ color: '#4da6ff' }}>TEC</span>Air
          </div>
          <div className="text-white-50 small mt-1">Panel de Aeropuerto</div>
        </div>

        <nav className="flex-grow-1">
          {NAV_ITEMS.map(item => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/aeropuerto'}
              className={({ isActive }) =>
                `d-flex align-items-center gap-2 px-3 py-2 mb-1 rounded text-decoration-none small fw-medium ${
                  isActive ? 'bg-primary text-white' : 'text-white-50'
                }`
              }
            >
              <i className={`bi ${item.icon}`}></i>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="border-top border-secondary pt-3 mt-3">
          <div className="px-2 mb-3">
            <div className="small text-white fw-medium">{user?.nombre}</div>
            <div className="text-white-50 small">{user?.rol}</div>
          </div>
          <button
            className="btn btn-outline-secondary btn-sm w-100"
            onClick={handleLogout}
          >
            <i className="bi bi-box-arrow-right me-2"></i>Cerrar sesión
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-grow-1 bg-light overflow-auto">
        <div className="p-4">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

export default AirportLayout;

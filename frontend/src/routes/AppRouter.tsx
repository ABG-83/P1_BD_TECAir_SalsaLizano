import { Routes, Route } from 'react-router-dom';
import HomeView from '../views/HomeView';
import ReservationView from '../views/ReservationView';
import CheckInView from '../views/CheckInView';
import LoginView from '../views/LoginView';
import RegisterView from '../views/RegisterView';
import PromotionsView from '../views/PromotionsView';
import MyReservationsView from '../views/MyReservationsView';
import PaymentView from '../views/PaymentView';
import ProtectedRoute from '../components/ProtectedRoute';
import AirportLayout from '../views/airport/AirportLayout';
import AirportDashboard from '../views/airport/AirportDashboard';
import FlightManagementView from '../views/airport/FlightManagementView';
import UserManagementView from '../views/airport/UserManagementView';
import CheckInManagementView from '../views/airport/CheckInManagementView';
import PromotionManagementView from '../views/airport/PromotionManagementView';
import ReservationManagementView from '../views/airport/ReservationManagementView';

const STAFF_ROLES = ['funcionario', 'administrador'] as const;

const AppRouter = () => {
  return (
    <Routes>
      {/* Plataforma Reservaciones (cliente) */}
      <Route path="/" element={<HomeView />} />
      <Route path="/reservacion" element={<ReservationView />} />
      <Route path="/checkin" element={<CheckInView />} />
      <Route path="/login" element={<LoginView />} />
      <Route path="/registro" element={<RegisterView />} />
      <Route path="/promociones" element={<PromotionsView />} />
      <Route path="/pago" element={<PaymentView />} />
      <Route
        path="/mis-reservaciones"
        element={
          <ProtectedRoute>
            <MyReservationsView />
          </ProtectedRoute>
        }
      />

      {/* Plataforma Aeropuerto (funcionario / administrador) */}
      <Route
        path="/aeropuerto"
        element={
          <ProtectedRoute requiredRole={[...STAFF_ROLES]}>
            <AirportLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<AirportDashboard />} />
        <Route path="checkin" element={<CheckInManagementView />} />
        <Route path="vuelos" element={
          <ProtectedRoute requiredRole="administrador"><FlightManagementView /></ProtectedRoute>
        } />
        <Route path="usuarios" element={
          <ProtectedRoute requiredRole="administrador"><UserManagementView /></ProtectedRoute>
        } />
        <Route path="promociones" element={
          <ProtectedRoute requiredRole="administrador"><PromotionManagementView /></ProtectedRoute>
        } />
        <Route path="reservaciones" element={
          <ProtectedRoute requiredRole="administrador"><ReservationManagementView /></ProtectedRoute>
        } />
      </Route>
    </Routes>
  );
};

export default AppRouter;

import { Routes, Route } from 'react-router-dom';
import HomeView from '../views/HomeView';
import ReservationView from '../views/ReservationView';
import CheckInView from '../views/CheckInView';
import LoginView from '../views/LoginView';

const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<HomeView />} />
      <Route path="/reservacion" element={<ReservationView />} />
      <Route path="/checkin" element={<CheckInView />} />
      <Route path="/login" element={<LoginView />} />
    </Routes>
  );
};

export default AppRouter;

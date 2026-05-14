import { Routes, Route } from 'react-router-dom';
import HomeView from '../views/HomeView';
import ReservationView from '../views/ReservationView';
import CheckInView from '../views/CheckInView';
import LoginView from '../views/LoginView';
import RegisterView from '../views/RegisterView';
import PromotionsView from '../views/PromotionsView';

const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<HomeView />} />
      <Route path="/reservacion" element={<ReservationView />} />
      <Route path="/checkin" element={<CheckInView />} />
      <Route path="/login" element={<LoginView />} />
      <Route path="/registro" element={<RegisterView />} />
      <Route path="/promociones" element={<PromotionsView />} />
    </Routes>
  );
};

export default AppRouter;

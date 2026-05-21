import { useLocation } from 'react-router-dom';
import AppRouter from './routes/AppRouter';
import Navbar from './components/Navbar';
import Footer from './components/Footer';

const AIRPORT_PREFIX = '/aeropuerto';

function App() {
  const { pathname } = useLocation();
  const isAirport = pathname.startsWith(AIRPORT_PREFIX);

  return (
    <div className="d-flex flex-column min-vh-100">
      {!isAirport && <Navbar />}
      <main className="flex-grow-1">
        <AppRouter />
      </main>
      {!isAirport && <Footer />}
    </div>
  );
}

export default App;

import AppRouter from './routes/AppRouter';
import Navbar from './components/Navbar';
import Footer from './components/Footer';

function App() {
  return (
    <div className="d-flex flex-column min-vh-100">
      <Navbar />
      <main className="flex-grow-1">
        <AppRouter />
      </main>
      <Footer />
    </div>
  );
}

export default App;

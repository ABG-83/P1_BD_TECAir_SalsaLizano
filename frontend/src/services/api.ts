import axios from 'axios';

// Configuración base de Axios
// Por ahora apuntará a un endpoint local o puede ser reemplazado por los mocks
const api = axios.create({
  baseURL: 'http://localhost:5102/api', // Ajustar al puerto de la Web API C#
  headers: {
    'Content-Type': 'application/json'
  }
});

export default api;

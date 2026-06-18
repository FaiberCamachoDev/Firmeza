import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5109';

const apiClient = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
  // H5: withCredentials envía la cookie httpOnly automáticamente — el token
  // nunca pasa por JavaScript ni se guarda en localStorage
  withCredentials: true,
});

// Redirige al login cuando el token expira o es inválido
apiClient.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      sessionStorage.removeItem('firmeza_auth');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export default apiClient;

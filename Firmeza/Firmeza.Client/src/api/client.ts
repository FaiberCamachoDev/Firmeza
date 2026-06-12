import axios from 'axios';

const API_BASE = 'http://localhost:5109';

const apiClient = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

// Adjunta el JWT en cada request si existe
apiClient.interceptors.request.use((config) => {
  const raw = localStorage.getItem('firmeza_auth');
  if (raw) {
    const auth = JSON.parse(raw);
    if (auth?.token) {
      config.headers.Authorization = `Bearer ${auth.token}`;
    }
  }
  return config;
});

// Redirige al login cuando el token expira o es inválido
apiClient.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('firmeza_auth');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export default apiClient;

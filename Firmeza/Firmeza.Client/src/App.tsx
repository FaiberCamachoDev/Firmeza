import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import PrivateRoute from './components/PrivateRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CatalogPage from './pages/CatalogPage';
import CartPage from './pages/CartPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route
              path="/catalog"
              element={<PrivateRoute><CatalogPage /></PrivateRoute>}
            />
            <Route
              path="/cart"
              element={<PrivateRoute><CartPage /></PrivateRoute>}
            />
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

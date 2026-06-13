import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

export default function Navbar() {
  const { user, logout } = useAuth();
  const { totalItems } = useCart();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="bg-indigo-700 text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <Link to="/catalog" className="text-xl font-bold tracking-wide hover:text-indigo-200 transition">
            Firmeza
          </Link>

          <div className="flex items-center gap-6">
            <Link to="/catalog" className="hover:text-indigo-200 transition text-sm font-medium">
              Productos
            </Link>

            <Link to="/cart" className="relative hover:text-indigo-200 transition">
              <span className="text-sm font-medium">Carrito</span>
              {totalItems > 0 && (
                <span className="absolute -top-2 -right-3 bg-orange-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
                  {totalItems}
                </span>
              )}
            </Link>

            {user && (
              <div className="flex items-center gap-3 border-l border-indigo-500 pl-6">
                <span className="text-sm text-indigo-200">Hola, {user.fullName.split(' ')[0]}</span>
                <button
                  onClick={handleLogout}
                  className="text-sm bg-indigo-600 hover:bg-indigo-500 px-3 py-1.5 rounded-md transition"
                >
                  Cerrar sesión
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}

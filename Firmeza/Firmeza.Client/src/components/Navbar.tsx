import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

export default function Navbar() {
  const { user, logout } = useAuth();
  const { totalItems } = useCart();
  const navigate = useNavigate();
  const location = useLocation();
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    const handler = () => setScrolled(window.scrollY > 12);
    window.addEventListener('scroll', handler, { passive: true });
    return () => window.removeEventListener('scroll', handler);
  }, []);

  useEffect(() => { setMenuOpen(false); }, [location.pathname]);

  const handleLogout = () => { logout(); navigate('/login'); };
  const isActive = (path: string) => location.pathname === path;

  return (
    <nav
      className="sticky top-0 z-50 w-full transition-all duration-200"
      style={{
        background: scrolled || menuOpen ? 'rgba(12,12,16,0.98)' : '#0C0C10',
        borderBottom: `1px solid ${scrolled || menuOpen ? '#3E3E58' : '#2A2A38'}`,
        backdropFilter: scrolled ? 'blur(12px)' : 'none',
        boxShadow: scrolled ? '0 4px 24px rgba(0,0,0,0.5)' : 'none',
      }}
    >
      {/* Inner container — centered, with generous side padding */}
      <div className="w-full max-w-5xl mx-auto px-8 sm:px-12 lg:px-16">
        <div className="flex items-center justify-between h-16">

          {/* Logo */}
          <Link to="/catalog" className="flex items-center gap-3 group shrink-0">
            <div
              className="w-8 h-8 rounded-lg flex items-center justify-center"
              style={{ background: 'linear-gradient(135deg, #F59E0B, #EA580C)' }}
            >
              <span className="text-black font-extrabold text-sm" style={{ fontFamily: 'Barlow Condensed, sans-serif' }}>F</span>
            </div>
            <span
              className="text-lg font-extrabold tracking-widest transition-colors group-hover:text-amber-400"
              style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F8FAFC' }}
            >
              FIRMEZA
            </span>
          </Link>

          {/* Desktop nav links */}
          <div className="hidden sm:flex items-center gap-1">
            <NavLink to="/catalog" active={isActive('/catalog')} label="Productos" />

            <Link
              to="/cart"
              className="relative flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-colors"
              style={{ color: isActive('/cart') ? '#F59E0B' : '#94A3B8' }}
            >
              <CartIcon className="w-4 h-4" />
              <span>Carrito</span>
              <AnimatePresence>
                {totalItems > 0 && (
                  <motion.span
                    key="badge"
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    exit={{ scale: 0 }}
                    className="flex items-center justify-center w-5 h-5 rounded-full text-xs font-bold"
                    style={{ background: '#F59E0B', color: '#0C0C10' }}
                  >
                    {totalItems}
                  </motion.span>
                )}
              </AnimatePresence>
            </Link>

            {user && (
              <div className="flex items-center gap-3 ml-4 pl-4" style={{ borderLeft: '1px solid #2A2A38' }}>
                <div
                  className="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold"
                  style={{ background: '#202029', color: '#F59E0B', border: '1px solid #3E3E58' }}
                >
                  {user.fullName.charAt(0).toUpperCase()}
                </div>
                <span className="text-sm hidden md:block" style={{ color: '#64748B' }}>
                  {user.fullName.split(' ')[0]}
                </span>
                <button onClick={handleLogout} className="btn-ghost text-xs px-3 py-2 ml-1">
                  Salir
                </button>
              </div>
            )}
          </div>

          {/* Mobile: cart + hamburger */}
          <div className="flex sm:hidden items-center gap-2">
            <Link
              to="/cart"
              className="relative p-2 rounded-lg"
              style={{ color: isActive('/cart') ? '#F59E0B' : '#94A3B8' }}
            >
              <CartIcon className="w-5 h-5" />
              {totalItems > 0 && (
                <span
                  className="absolute -top-0.5 -right-0.5 w-4 h-4 rounded-full flex items-center justify-center font-bold"
                  style={{ background: '#F59E0B', color: '#0C0C10', fontSize: '10px' }}
                >
                  {totalItems > 9 ? '9+' : totalItems}
                </span>
              )}
            </Link>
            <button
              onClick={() => setMenuOpen((v) => !v)}
              className="p-2 rounded-lg transition-colors"
              style={{ color: '#94A3B8', background: menuOpen ? '#202029' : 'transparent' }}
              aria-label={menuOpen ? 'Cerrar menú' : 'Abrir menú'}
            >
              <AnimatePresence mode="wait" initial={false}>
                {menuOpen ? (
                  <motion.span key="x" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }} transition={{ duration: 0.15 }}>
                    <XIcon className="w-5 h-5" />
                  </motion.span>
                ) : (
                  <motion.span key="menu" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }} transition={{ duration: 0.15 }}>
                    <MenuIcon className="w-5 h-5" />
                  </motion.span>
                )}
              </AnimatePresence>
            </button>
          </div>
        </div>
      </div>

      {/* Mobile dropdown */}
      <AnimatePresence>
        {menuOpen && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.22, ease: 'easeOut' }}
            className="sm:hidden overflow-hidden"
            style={{ borderTop: '1px solid #2A2A38' }}
          >
            <div className="px-6 py-4 space-y-1 pb-5">
              <MobileLink to="/catalog" active={isActive('/catalog')} label="Productos" icon={<BoxIcon />} />
              <MobileLink
                to="/cart"
                active={isActive('/cart')}
                label="Carrito"
                icon={<CartIcon className="w-4 h-4" />}
                badge={totalItems > 0 ? totalItems : undefined}
              />
              {user && (
                <>
                  <div className="h-px mx-1 my-3" style={{ background: '#2A2A38' }} />
                  <div className="flex items-center gap-3 px-4 py-3.5 rounded-xl" style={{ background: '#111116', border: '1px solid #2A2A38' }}>
                    <div
                      className="w-9 h-9 rounded-full shrink-0 flex items-center justify-center font-bold text-sm"
                      style={{ background: '#202029', color: '#F59E0B', border: '1px solid #3E3E58' }}
                    >
                      {user.fullName.charAt(0).toUpperCase()}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold truncate" style={{ color: '#F8FAFC' }}>{user.fullName}</p>
                      <p className="text-xs truncate" style={{ color: '#475569' }}>{user.email}</p>
                    </div>
                    <button
                      onClick={handleLogout}
                      className="shrink-0 text-xs px-3 py-1.5 rounded-lg font-semibold"
                      style={{ color: '#EF4444', background: 'rgba(239,68,68,0.08)', border: '1px solid rgba(239,68,68,0.2)' }}
                    >
                      Salir
                    </button>
                  </div>
                </>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </nav>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────

function NavLink({ to, active, label }: { to: string; active: boolean; label: string }) {
  return (
    <Link
      to={to}
      className="relative px-4 py-2.5 text-sm font-medium transition-colors rounded-lg"
      style={{ color: active ? '#F59E0B' : '#94A3B8' }}
    >
      {label}
      {active && (
        <motion.div
          layoutId="nav-indicator"
          className="absolute bottom-0 left-4 right-4 h-0.5 rounded-full"
          style={{ background: '#F59E0B' }}
        />
      )}
    </Link>
  );
}

function MobileLink({ to, active, label, icon, badge }: {
  to: string; active: boolean; label: string; icon: React.ReactNode; badge?: number;
}) {
  return (
    <Link
      to={to}
      className="flex items-center gap-3 px-4 py-3.5 rounded-xl text-sm font-medium transition-colors"
      style={{
        color: active ? '#F59E0B' : '#94A3B8',
        background: active ? 'rgba(245,158,11,0.07)' : 'transparent',
        border: active ? '1px solid rgba(245,158,11,0.15)' : '1px solid transparent',
      }}
    >
      <span className="w-4 h-4 shrink-0">{icon}</span>
      <span className="flex-1">{label}</span>
      {badge !== undefined && (
        <span className="flex items-center justify-center w-5 h-5 rounded-full text-xs font-bold" style={{ background: '#F59E0B', color: '#0C0C10' }}>
          {badge}
        </span>
      )}
    </Link>
  );
}

// ── Icons ─────────────────────────────────────────────────────────────────────

function CartIcon({ className = 'w-4 h-4' }: { className?: string }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
    </svg>
  );
}

function MenuIcon({ className = 'w-5 h-5' }: { className?: string }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
    </svg>
  );
}

function XIcon({ className = 'w-5 h-5' }: { className?: string }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
    </svg>
  );
}

function BoxIcon() {
  return (
    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
    </svg>
  );
}

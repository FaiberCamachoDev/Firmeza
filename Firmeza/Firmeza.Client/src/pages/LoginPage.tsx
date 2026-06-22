import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { login } from '../api/auth';
import { useAuth } from '../context/AuthContext';
import PageWrapper from '../components/PageWrapper';

export default function LoginPage() {
  const { saveAuth } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const data = await login(form);
      saveAuth(data);
      navigate('/catalog');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })
        ?.response?.data?.message;
      setError(msg ?? 'Credenciales inválidas. Intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageWrapper>
      <div className="min-h-screen flex" style={{ background: '#0C0C10' }}>

        {/* Left panel — brand */}
        <div className="hidden lg:flex lg:w-1/2 flex-col justify-between p-12 relative overflow-hidden"
          style={{ background: 'linear-gradient(145deg, #111116 0%, #0C0C10 60%, #151510 100%)' }}>

          {/* Background geometric accent */}
          <div className="absolute inset-0 pointer-events-none" aria-hidden>
            <div className="absolute top-0 right-0 w-72 h-72 rounded-full opacity-10"
              style={{ background: 'radial-gradient(circle, #F59E0B 0%, transparent 70%)', transform: 'translate(30%, -30%)' }} />
            <div className="absolute bottom-0 left-0 w-96 h-96 rounded-full opacity-5"
              style={{ background: 'radial-gradient(circle, #EA580C 0%, transparent 70%)', transform: 'translate(-40%, 40%)' }} />
            {/* Grid lines */}
            <svg className="absolute inset-0 w-full h-full opacity-[0.03]" xmlns="http://www.w3.org/2000/svg">
              <defs>
                <pattern id="grid" width="48" height="48" patternUnits="userSpaceOnUse">
                  <path d="M 48 0 L 0 0 0 48" fill="none" stroke="#F59E0B" strokeWidth="0.5"/>
                </pattern>
              </defs>
              <rect width="100%" height="100%" fill="url(#grid)" />
            </svg>
          </div>

          {/* Logo top-left */}
          <div className="relative z-10 flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl flex items-center justify-center"
              style={{ background: 'linear-gradient(135deg, #F59E0B, #EA580C)' }}>
              <span className="text-black font-extrabold text-base" style={{ fontFamily: 'Barlow Condensed, sans-serif' }}>F</span>
            </div>
            <span className="text-white font-bold text-lg" style={{ fontFamily: 'Barlow Condensed, sans-serif', letterSpacing: '0.08em' }}>
              FIRMEZA
            </span>
          </div>

          {/* Center brand copy */}
          <div className="relative z-10">
            <motion.h1
              initial={{ opacity: 0, x: -24 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.15, duration: 0.5, ease: 'easeOut' }}
              className="font-extrabold leading-none mb-6"
              style={{
                fontFamily: 'Barlow Condensed, sans-serif',
                fontSize: 'clamp(3rem, 6vw, 5rem)',
                color: '#F8FAFC',
                letterSpacing: '-0.01em',
              }}
            >
              MATERIALES<br/>
              <span style={{ color: '#F59E0B' }}>PARA CONSTRUIR</span><br/>
              LO QUE IMPORTA.
            </motion.h1>
            <motion.p
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.35, duration: 0.4 }}
              className="text-base leading-relaxed"
              style={{ color: '#64748B', maxWidth: '360px' }}
            >
              Herramientas, maquinaria y materiales de construcción al alcance de tu mano.
            </motion.p>
          </div>

          {/* Bottom metadata */}
          <div className="relative z-10 flex gap-8">
            {[['+ 500', 'Productos'], ['24/7', 'Disponible'], ['100%', 'Garantizado']].map(([num, label]) => (
              <div key={label}>
                <p className="text-xl font-bold" style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F59E0B' }}>{num}</p>
                <p className="text-xs" style={{ color: '#475569' }}>{label}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Right panel — form */}
        <div className="flex-1 flex items-center justify-center px-6 sm:px-10 py-12 sm:py-16"
          style={{ background: '#0C0C10' }}>
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.35, ease: 'easeOut' }}
            className="w-full max-w-md"
          >

            {/* Mobile logo */}
            <div className="lg:hidden text-center mb-8">
              <div className="w-12 h-12 rounded-xl flex items-center justify-center mx-auto mb-3"
                style={{ background: 'linear-gradient(135deg, #F59E0B, #EA580C)' }}>
                <span className="text-black font-extrabold text-lg" style={{ fontFamily: 'Barlow Condensed, sans-serif' }}>F</span>
              </div>
              <h1 className="text-2xl font-bold" style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F8FAFC', letterSpacing: '0.06em' }}>
                FIRMEZA
              </h1>
              <p className="text-sm mt-1" style={{ color: '#475569' }}>Materiales de Construcción</p>
            </div>

            {/* Form card */}
            <div className="rounded-2xl p-8" style={{ background: '#111116', border: '1px solid #2A2A38' }}>
              <h2 className="text-xl font-semibold mb-6" style={{ color: '#F8FAFC' }}>
                Iniciar sesión
              </h2>

              {error && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  className="mb-5 text-sm px-4 py-3 rounded-lg"
                  style={{ background: 'rgba(239,68,68,0.1)', border: '1px solid rgba(239,68,68,0.25)', color: '#FCA5A5' }}
                >
                  {error}
                </motion.div>
              )}

              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider mb-2" style={{ color: '#64748B' }}>
                    Correo electrónico
                  </label>
                  <input
                    type="email"
                    required
                    value={form.email}
                    onChange={(e) => setForm({ ...form, email: e.target.value })}
                    placeholder="tu@correo.com"
                    className="input-dark"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider mb-2" style={{ color: '#64748B' }}>
                    Contraseña
                  </label>
                  <input
                    type="password"
                    required
                    value={form.password}
                    onChange={(e) => setForm({ ...form, password: e.target.value })}
                    placeholder="••••••••"
                    className="input-dark"
                  />
                </div>

                <div className="pt-2">
                  <motion.button
                    type="submit"
                    disabled={loading}
                    whileTap={{ scale: 0.97 }}
                    className="btn-amber w-full py-3 text-base"
                  >
                    {loading ? 'Ingresando...' : 'Ingresar'}
                  </motion.button>
                </div>
              </form>

              <p className="text-center text-sm mt-6" style={{ color: '#475569' }}>
                ¿No tienes cuenta?{' '}
                <Link to="/register" className="font-semibold transition-colors" style={{ color: '#F59E0B' }}>
                  Regístrate
                </Link>
              </p>
            </div>
          </motion.div>
        </div>
      </div>
    </PageWrapper>
  );
}

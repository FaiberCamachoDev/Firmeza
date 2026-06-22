import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { register } from '../api/auth';
import { useAuth } from '../context/AuthContext';
import PageWrapper from '../components/PageWrapper';

const IDENTITY_ERRORS: Record<string, string> = {
  'Passwords must have at least one non alphanumeric character.':
    'La contraseña debe tener al menos un carácter especial (ej: ! @ # $ %).',
  'Passwords must have at least one digit': 'La contraseña debe tener al menos un número.',
  'Passwords must have at least one uppercase': 'La contraseña debe tener al menos una mayúscula.',
  'Passwords must be at least': 'La contraseña debe tener al menos 8 caracteres.',
};

function translateError(msg: string): string {
  for (const [key, val] of Object.entries(IDENTITY_ERRORS)) {
    if (msg.includes(key)) return val;
  }
  return msg;
}

export default function RegisterPage() {
  const { saveAuth } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    documentNumber: '',
    phone: '',
  });
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  const set = (field: string, value: string) => setForm((f) => ({ ...f, [field]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    try {
      const data = await register(form);
      saveAuth(data);
      navigate('/catalog');
    } catch (err: unknown) {
      const res = (err as { response?: { data?: { message?: string; errors?: string[] } } })?.response?.data;
      if (res?.errors) setErrors(res.errors.map(translateError));
      else setErrors([translateError(res?.message ?? 'Error al registrarse.')]);
    } finally {
      setLoading(false);
    }
  };

  const inputField = (label: string, name: string, type = 'text', placeholder = '') => (
    <div>
      <label className="block text-xs font-semibold uppercase tracking-wider mb-2" style={{ color: '#64748B' }}>
        {label}
      </label>
      <input
        type={type}
        required
        value={form[name as keyof typeof form]}
        onChange={(e) => set(name, e.target.value)}
        placeholder={placeholder}
        className="input-dark"
      />
    </div>
  );

  return (
    <PageWrapper>
      <div className="min-h-screen flex items-center justify-center px-5 sm:px-8 py-12 sm:py-16" style={{ background: '#0C0C10' }}>
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.35, ease: 'easeOut' }}
          className="w-full max-w-lg"
        >

          {/* Header */}
          <div className="text-center mb-8">
            <Link to="/login" className="inline-flex items-center gap-2.5 mb-4">
              <div className="w-10 h-10 rounded-xl flex items-center justify-center"
                style={{ background: 'linear-gradient(135deg, #F59E0B, #EA580C)' }}>
                <span className="text-black font-extrabold text-base" style={{ fontFamily: 'Barlow Condensed, sans-serif' }}>F</span>
              </div>
              <span className="text-xl font-bold" style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F8FAFC', letterSpacing: '0.06em' }}>
                FIRMEZA
              </span>
            </Link>
            <h1 className="text-2xl font-bold" style={{ color: '#F8FAFC' }}>Crear cuenta</h1>
            <p className="text-sm mt-1" style={{ color: '#475569' }}>Completa tus datos para comenzar</p>
          </div>

          {/* Form card */}
          <div className="rounded-2xl p-8" style={{ background: '#111116', border: '1px solid #2A2A38' }}>

            {errors.length > 0 && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="mb-5 text-sm px-4 py-3 rounded-lg space-y-1"
                style={{ background: 'rgba(239,68,68,0.1)', border: '1px solid rgba(239,68,68,0.25)', color: '#FCA5A5' }}
              >
                {errors.map((e, i) => <p key={i}>{e}</p>)}
              </motion.div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                {inputField('Nombre', 'firstName', 'text', 'Juan')}
                {inputField('Apellido', 'lastName', 'text', 'Pérez')}
              </div>

              {inputField('Correo electrónico', 'email', 'email', 'tu@correo.com')}

              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider mb-2" style={{ color: '#64748B' }}>
                  Contraseña
                </label>
                <input
                  type="password"
                  required
                  value={form.password}
                  onChange={(e) => set('password', e.target.value)}
                  placeholder="Ej: MiClave2024!"
                  className="input-dark"
                />
                <p className="text-xs mt-1.5" style={{ color: '#334155' }}>
                  Mín. 8 caracteres · una mayúscula · un número · un símbolo (<span className="font-mono">! @ # $</span>)
                </p>
              </div>

              {inputField('Número de documento', 'documentNumber', 'text', 'CC / RUC / Pasaporte')}
              {inputField('Teléfono', 'phone', 'tel', '0999-000-000')}

              <div className="pt-2">
                <motion.button
                  type="submit"
                  disabled={loading}
                  whileTap={{ scale: 0.97 }}
                  className="btn-amber w-full py-3 text-base"
                >
                  {loading ? 'Creando cuenta...' : 'Crear cuenta'}
                </motion.button>
              </div>
            </form>

            <p className="text-center text-sm mt-6" style={{ color: '#475569' }}>
              ¿Ya tienes cuenta?{' '}
              <Link to="/login" className="font-semibold" style={{ color: '#F59E0B' }}>
                Inicia sesión
              </Link>
            </p>
          </div>
        </motion.div>
      </div>
    </PageWrapper>
  );
}

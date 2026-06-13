import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { register } from '../api/auth';
import { useAuth } from '../context/AuthContext';

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

  const field = (label: string, name: string, type = 'text', placeholder = '') => (
    <div>
      <label className="block text-sm font-medium text-slate-700 mb-1">{label}</label>
      <input
        type={type}
        required
        value={form[name as keyof typeof form]}
        onChange={(e) => set(name, e.target.value)}
        placeholder={placeholder}
        className="w-full border border-slate-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
      />
    </div>
  );

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-50 to-slate-100 px-4 py-10">
      <div className="w-full max-w-lg bg-white rounded-2xl shadow-lg p-8">
        <div className="text-center mb-6">
          <h1 className="text-3xl font-bold text-indigo-700">Firmeza</h1>
          <p className="text-slate-500 mt-1 text-sm">Crea tu cuenta de cliente</p>
        </div>

        {errors.length > 0 && (
          <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-3 space-y-1">
            {errors.map((e, i) => <p key={i}>{e}</p>)}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            {field('Nombre', 'firstName', 'text', 'Juan')}
            {field('Apellido', 'lastName', 'text', 'Pérez')}
          </div>
          {field('Correo electrónico', 'email', 'email', 'tu@correo.com')}

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Contraseña</label>
            <input
              type="password"
              required
              value={form.password}
              onChange={(e) => set('password', e.target.value)}
              placeholder="Ej: MiClave2024!"
              className="w-full border border-slate-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <p className="text-xs text-slate-400 mt-1.5">
              Mínimo 8 caracteres · una mayúscula · un número · un símbolo (<span className="font-mono">! @ # $ %</span>)
            </p>
          </div>

          {field('Número de documento', 'documentNumber', 'text', 'CC / RUC / Pasaporte')}
          {field('Teléfono', 'phone', 'tel', '0999-000-000')}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2.5 rounded-lg transition disabled:opacity-60 mt-2"
          >
            {loading ? 'Registrando...' : 'Crear cuenta'}
          </button>
        </form>

        <p className="text-center text-sm text-slate-500 mt-6">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="text-indigo-600 font-medium hover:underline">
            Inicia sesión
          </Link>
        </p>
      </div>
    </div>
  );
}

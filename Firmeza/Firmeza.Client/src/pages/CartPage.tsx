import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { createSale } from '../api/sales';
import type { SaleDto } from '../types';
import Navbar from '../components/Navbar';

export default function CartPage() {
  const { items, subtotal, tax, total, removeFromCart, updateQuantity, clearCart } = useCart();
  const { user } = useAuth();
  const navigate = useNavigate();

  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [confirmation, setConfirmation] = useState<SaleDto | null>(null);
  const [error, setError] = useState('');

  const handleCheckout = async () => {
    if (!user?.customerId) {
      setError('No se encontró tu registro de cliente. Por favor cierra sesión y vuelve a registrarte.');
      return;
    }
    if (items.length === 0) return;

    setError('');
    setLoading(true);
    try {
      const sale = await createSale({
        customerId: user.customerId,
        notes,
        items: items.map((i) => ({ productId: i.product.id, quantity: i.quantity })),
      });
      clearCart();
      setConfirmation(sale);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg ?? 'Error al procesar la compra.');
    } finally {
      setLoading(false);
    }
  };

  if (confirmation) {
    return (
      <div className="min-h-screen bg-slate-50">
        <Navbar />
        <main className="max-w-2xl mx-auto px-4 py-16">
          <div className="bg-white rounded-2xl shadow-lg p-8 text-center">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-slate-800 mb-2">¡Compra realizada!</h2>
            <p className="text-slate-500 mb-1">Orden <span className="font-semibold text-indigo-600">#{String(confirmation.id).padStart(6, '0')}</span></p>
            <p className="text-slate-500 mb-6">
              Se envió un comprobante a <strong>{user?.email}</strong> con el detalle de tu compra.
            </p>

            <div className="bg-slate-50 rounded-xl p-4 text-left mb-6">
              {confirmation.details.map((d) => (
                <div key={d.productId} className="flex justify-between text-sm py-1.5 border-b border-slate-100 last:border-0">
                  <span className="text-slate-700">{d.productName} × {d.quantity}</span>
                  <span className="font-medium">${d.subtotal.toFixed(2)}</span>
                </div>
              ))}
              <div className="flex justify-between font-bold text-slate-800 mt-3 pt-2 border-t border-slate-200">
                <span>Total</span>
                <span>${confirmation.total.toFixed(2)}</span>
              </div>
            </div>

            <button
              onClick={() => navigate('/catalog')}
              className="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold px-6 py-2.5 rounded-lg transition"
            >
              Seguir comprando
            </button>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <Navbar />
      <main className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-2xl font-bold text-slate-800 mb-6">Mi Carrito</h1>

        {items.length === 0 ? (
          <div className="text-center py-16 bg-white rounded-2xl shadow-sm">
            <p className="text-slate-400 mb-4">Tu carrito está vacío.</p>
            <Link to="/catalog" className="text-indigo-600 font-medium hover:underline text-sm">
              Ver catálogo →
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Tabla de items */}
            <div className="lg:col-span-2 space-y-3">
              {items.map((item) => (
                <div key={item.product.id} className="bg-white rounded-xl shadow-sm border border-slate-100 p-4 flex items-center gap-4">
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-slate-800 truncate">{item.product.name}</p>
                    <p className="text-sm text-indigo-600">${item.product.price.toFixed(2)} / {item.product.unit}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => item.quantity > 1 ? updateQuantity(item.product.id, item.quantity - 1) : removeFromCart(item.product.id)}
                      className="w-7 h-7 rounded-full border border-slate-300 text-slate-600 hover:bg-slate-100 flex items-center justify-center text-sm font-bold transition"
                    >−</button>
                    <span className="w-8 text-center text-sm font-semibold">{item.quantity}</span>
                    <button
                      onClick={() => updateQuantity(item.product.id, Math.min(item.quantity + 1, item.product.stock))}
                      disabled={item.quantity >= item.product.stock}
                      className="w-7 h-7 rounded-full border border-slate-300 text-slate-600 hover:bg-slate-100 flex items-center justify-center text-sm font-bold disabled:opacity-40 transition"
                    >+</button>
                  </div>
                  <p className="w-20 text-right font-semibold text-slate-700">
                    ${(item.product.price * item.quantity).toFixed(2)}
                  </p>
                  <button
                    onClick={() => removeFromCart(item.product.id)}
                    className="text-red-400 hover:text-red-600 transition ml-1"
                    title="Eliminar"
                  >
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              ))}
            </div>

            {/* Resumen y checkout */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 sticky top-6">
                <h2 className="text-lg font-bold text-slate-800 mb-4">Resumen</h2>
                <div className="space-y-2 text-sm text-slate-600">
                  <div className="flex justify-between">
                    <span>Subtotal</span>
                    <span>${subtotal.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>IVA (12%)</span>
                    <span>${tax.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between font-bold text-slate-800 text-base pt-2 border-t border-slate-200">
                    <span>Total</span>
                    <span>${total.toFixed(2)}</span>
                  </div>
                </div>

                <div className="mt-4">
                  <label className="block text-sm font-medium text-slate-700 mb-1">Notas (opcional)</label>
                  <textarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    rows={2}
                    placeholder="Instrucciones de entrega..."
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                  />
                </div>

                {error && (
                  <p className="text-xs text-red-600 bg-red-50 rounded-lg px-3 py-2 mt-3">{error}</p>
                )}

                <button
                  onClick={handleCheckout}
                  disabled={loading || items.length === 0}
                  className="w-full mt-4 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2.5 rounded-lg transition disabled:opacity-60"
                >
                  {loading ? 'Procesando...' : 'Confirmar compra'}
                </button>

                <Link to="/catalog" className="block text-center text-xs text-slate-400 hover:text-slate-600 mt-3 transition">
                  ← Seguir comprando
                </Link>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}

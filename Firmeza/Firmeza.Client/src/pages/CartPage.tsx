import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { createSale } from '../api/sales';
import type { SaleDto } from '../types';
import Navbar from '../components/Navbar';
import PageWrapper from '../components/PageWrapper';

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

  // ── Confirmación ────────────────────────────────────────────────────────────
  if (confirmation) {
    return (
      <PageWrapper className="min-h-screen" style={{ background: '#0C0C10' } as React.CSSProperties}>
        <Navbar />
        <main className="w-full"><div className="max-w-lg mx-auto px-5 py-12 sm:py-16">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 16 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            transition={{ duration: 0.4, ease: 'easeOut' }}
            className="rounded-2xl p-6 sm:p-8 text-center"
            style={{ background: '#111116', border: '1px solid #2A2A38' }}
          >
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{ delay: 0.2, type: 'spring', stiffness: 280 }}
              className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-5"
              style={{ background: 'rgba(34,197,94,0.12)', border: '1px solid rgba(34,197,94,0.3)' }}
            >
              <svg className="w-8 h-8" style={{ color: '#22C55E' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
              </svg>
            </motion.div>

            <h2 className="text-2xl font-bold mb-1" style={{ color: '#F8FAFC' }}>¡Compra confirmada!</h2>
            <p className="text-sm mb-0.5" style={{ color: '#64748B' }}>
              Orden{' '}
              <span className="font-bold" style={{ color: '#F59E0B' }}>
                #{String(confirmation.id).padStart(6, '0')}
              </span>
            </p>
            <p className="text-sm mb-6" style={{ color: '#475569' }}>
              Comprobante enviado a{' '}
              <strong style={{ color: '#94A3B8' }}>{user?.email}</strong>
            </p>

            <div className="rounded-xl p-4 text-left mb-6" style={{ background: '#0C0C10', border: '1px solid #2A2A38' }}>
              {confirmation.details.map((d) => (
                <div
                  key={d.productId}
                  className="flex justify-between text-sm py-2.5"
                  style={{ borderBottom: '1px solid #18181F' }}
                >
                  <span style={{ color: '#94A3B8' }}>{d.productName} × {d.quantity}</span>
                  <span className="font-semibold" style={{ color: '#F8FAFC' }}>${d.subtotal.toFixed(2)}</span>
                </div>
              ))}
              <div className="flex justify-between font-bold text-base mt-3 pt-2" style={{ borderTop: '1px solid #2A2A38' }}>
                <span style={{ color: '#F8FAFC' }}>Total</span>
                <span style={{ color: '#F59E0B', fontFamily: 'Barlow Condensed, sans-serif', fontSize: '1.3rem' }}>
                  ${confirmation.total.toFixed(2)}
                </span>
              </div>
            </div>

            <motion.button
              whileTap={{ scale: 0.97 }}
              onClick={() => navigate('/catalog')}
              className="btn-amber w-full py-3 text-base"
            >
              Seguir comprando
            </motion.button>
          </motion.div>
        </div></main>
      </PageWrapper>
    );
  }

  // ── Carrito vacío ────────────────────────────────────────────────────────────
  if (items.length === 0) {
    return (
      <PageWrapper className="min-h-screen" style={{ background: '#0C0C10' } as React.CSSProperties}>
        <Navbar />
        <main className="w-full"><div className="max-w-lg mx-auto px-5 py-16 sm:py-24 text-center">
          <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}>
            <div
              className="w-20 h-20 rounded-2xl flex items-center justify-center mx-auto mb-5"
              style={{ background: '#111116', border: '1px solid #2A2A38' }}
            >
              <svg className="w-9 h-9" style={{ color: '#2A2A38' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
            </div>
            <p className="text-lg font-semibold mb-2" style={{ color: '#475569' }}>Tu carrito está vacío</p>
            <p className="text-sm mb-6" style={{ color: '#334155' }}>Agrega productos desde el catálogo</p>
            <Link to="/catalog" className="btn-amber inline-block px-6 py-2.5 text-sm">
              Ver catálogo
            </Link>
          </motion.div>
        </div></main>
      </PageWrapper>
    );
  }

  // ── Carrito con items ────────────────────────────────────────────────────────
  return (
    <PageWrapper className="min-h-screen" style={{ background: '#0C0C10' } as React.CSSProperties}>
      <Navbar />
      <main className="w-full">
      <div className="max-w-5xl mx-auto px-8 sm:px-12 lg:px-16 py-10 sm:py-12">

        <div className="flex items-baseline gap-3 mb-6">
          <h1
            className="text-3xl font-extrabold"
            style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F8FAFC', letterSpacing: '0.01em' }}
          >
            MI CARRITO
          </h1>
          <span
            className="text-sm font-semibold px-2 py-0.5 rounded-full"
            style={{ background: 'rgba(245,158,11,0.12)', color: '#F59E0B', border: '1px solid rgba(245,158,11,0.2)' }}
          >
            {items.length} {items.length === 1 ? 'producto' : 'productos'}
          </span>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">

          {/* Items list */}
          <div className="lg:col-span-2 space-y-3">
            <AnimatePresence initial={false}>
              {items.map((item) => (
                <motion.div
                  key={item.product.id}
                  initial={{ opacity: 0, y: -8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, x: 24, height: 0, marginBottom: 0, paddingTop: 0, paddingBottom: 0 }}
                  transition={{ duration: 0.22 }}
                  className="rounded-xl overflow-hidden"
                  style={{ background: '#111116', border: '1px solid #2A2A38' }}
                >
                  <div className="p-5">
                    {/* Row 1: icon + name + price — always visible */}
                    <div className="flex items-start gap-3">
                      <div
                        className="w-10 h-10 rounded-lg shrink-0 flex items-center justify-center text-xs font-bold mt-0.5"
                        style={{ background: 'rgba(245,158,11,0.08)', color: '#F59E0B', border: '1px solid rgba(245,158,11,0.18)' }}
                      >
                        {item.product.category.charAt(0)}
                      </div>

                      <div className="flex-1 min-w-0">
                        <p className="font-semibold text-sm leading-tight" style={{ color: '#F1F5F9' }}>
                          {item.product.name}
                        </p>
                        <p className="text-xs mt-0.5" style={{ color: '#475569' }}>
                          ${item.product.price.toFixed(2)} / {item.product.unit}
                        </p>
                      </div>

                      {/* Subtotal — desktop only in this row */}
                      <p
                        className="hidden sm:block font-bold shrink-0"
                        style={{ fontFamily: 'Barlow Condensed, sans-serif', fontSize: '1.15rem', color: '#F59E0B' }}
                      >
                        ${(item.product.price * item.quantity).toFixed(2)}
                      </p>
                    </div>

                    {/* Row 2: qty controls + mobile subtotal + remove */}
                    <div className="flex items-center justify-between mt-3 pt-3" style={{ borderTop: '1px solid #1E1E26' }}>
                      {/* Qty controls */}
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() =>
                            item.quantity > 1
                              ? updateQuantity(item.product.id, item.quantity - 1)
                              : removeFromCart(item.product.id)
                          }
                          className="w-8 h-8 rounded-lg flex items-center justify-center text-base font-bold transition-colors"
                          style={{ background: '#202029', color: '#94A3B8', border: '1px solid #2A2A38' }}
                        >
                          −
                        </button>
                        <span className="w-8 text-center text-sm font-semibold" style={{ color: '#F8FAFC' }}>
                          {item.quantity}
                        </span>
                        <button
                          onClick={() => updateQuantity(item.product.id, Math.min(item.quantity + 1, item.product.stock))}
                          disabled={item.quantity >= item.product.stock}
                          className="w-8 h-8 rounded-lg flex items-center justify-center text-base font-bold transition-colors disabled:opacity-30"
                          style={{ background: '#202029', color: '#94A3B8', border: '1px solid #2A2A38' }}
                        >
                          +
                        </button>
                        <span className="text-xs ml-1" style={{ color: '#334155' }}>
                          (máx. {item.product.stock})
                        </span>
                      </div>

                      <div className="flex items-center gap-3">
                        {/* Subtotal — mobile only */}
                        <p
                          className="sm:hidden font-bold"
                          style={{ fontFamily: 'Barlow Condensed, sans-serif', fontSize: '1.1rem', color: '#F59E0B' }}
                        >
                          ${(item.product.price * item.quantity).toFixed(2)}
                        </p>

                        {/* Remove */}
                        <button
                          onClick={() => removeFromCart(item.product.id)}
                          className="w-8 h-8 rounded-lg flex items-center justify-center transition-colors"
                          style={{ background: '#1E1E26', color: '#334155', border: '1px solid #202029' }}
                          onMouseEnter={(e) => {
                            (e.currentTarget as HTMLElement).style.background = 'rgba(239,68,68,0.1)';
                            (e.currentTarget as HTMLElement).style.color = '#EF4444';
                            (e.currentTarget as HTMLElement).style.borderColor = 'rgba(239,68,68,0.25)';
                          }}
                          onMouseLeave={(e) => {
                            (e.currentTarget as HTMLElement).style.background = '#1E1E26';
                            (e.currentTarget as HTMLElement).style.color = '#334155';
                            (e.currentTarget as HTMLElement).style.borderColor = '#202029';
                          }}
                          aria-label="Eliminar"
                        >
                          <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>

          {/* Order summary */}
          <div className="lg:col-span-1">
            <div className="rounded-xl p-6 sm:p-7 lg:sticky lg:top-24" style={{ background: '#111116', border: '1px solid #2A2A38' }}>
              <h2 className="text-base font-bold mb-4" style={{ color: '#F8FAFC' }}>Resumen del pedido</h2>

              <div className="space-y-2.5 text-sm">
                <div className="flex justify-between">
                  <span style={{ color: '#64748B' }}>Subtotal</span>
                  <span style={{ color: '#94A3B8' }}>${subtotal.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span style={{ color: '#64748B' }}>IVA (12%)</span>
                  <span style={{ color: '#94A3B8' }}>${tax.toFixed(2)}</span>
                </div>
                <div
                  className="flex justify-between font-bold text-base pt-3 mt-1"
                  style={{ borderTop: '1px solid #2A2A38', color: '#F8FAFC' }}
                >
                  <span>Total</span>
                  <span style={{ fontFamily: 'Barlow Condensed, sans-serif', fontSize: '1.4rem', color: '#F59E0B' }}>
                    ${total.toFixed(2)}
                  </span>
                </div>
              </div>

              <div className="mt-5">
                <label
                  className="block text-xs font-semibold uppercase tracking-wider mb-2"
                  style={{ color: '#64748B' }}
                >
                  Notas (opcional)
                </label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={2}
                  placeholder="Instrucciones de entrega..."
                  className="input-dark resize-none"
                />
              </div>

              {error && (
                <motion.p
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  className="text-xs px-3 py-2.5 rounded-lg mt-3"
                  style={{
                    background: 'rgba(239,68,68,0.08)',
                    color: '#FCA5A5',
                    border: '1px solid rgba(239,68,68,0.2)',
                  }}
                >
                  {error}
                </motion.p>
              )}

              <motion.button
                onClick={handleCheckout}
                disabled={loading}
                whileTap={{ scale: 0.97 }}
                className="btn-amber w-full py-3 text-sm mt-4"
              >
                {loading ? 'Procesando...' : 'Confirmar compra'}
              </motion.button>

              <Link
                to="/catalog"
                className="block text-center text-xs mt-3 transition-colors"
                style={{ color: '#334155' }}
                onMouseEnter={(e) => { (e.currentTarget as HTMLElement).style.color = '#64748B'; }}
                onMouseLeave={(e) => { (e.currentTarget as HTMLElement).style.color = '#334155'; }}
              >
                ← Seguir comprando
              </Link>
            </div>
          </div>
        </div>
      </div></main>
    </PageWrapper>
  );
}

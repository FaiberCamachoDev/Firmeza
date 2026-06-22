import { motion } from 'framer-motion';
import type { Product } from '../types';
import { useCart } from '../context/CartContext';

interface Props {
  product: Product;
}

export const cardVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 },
};

const CATEGORY_META: Record<string, { bg: string; color: string; icon: string }> = {
  'Herramientas':  { bg: 'linear-gradient(135deg,#0f2744 0%,#071525 100%)', color: '#60A5FA', icon: '🔧' },
  'Maquinaria':    { bg: 'linear-gradient(135deg,#1e1040 0%,#0f0820 100%)', color: '#A78BFA', icon: '⚙️' },
  'Materiales':    { bg: 'linear-gradient(135deg,#2d1800 0%,#180d00 100%)', color: '#F59E0B', icon: '🧱' },
  'Eléctrico':     { bg: 'linear-gradient(135deg,#1a2e00 0%,#0d1800 100%)', color: '#84CC16', icon: '⚡' },
  'Plomería':      { bg: 'linear-gradient(135deg,#00281e 0%,#001510 100%)', color: '#34D399', icon: '🔩' },
  'Pintura':       { bg: 'linear-gradient(135deg,#2d0020 0%,#180010 100%)', color: '#F472B6', icon: '🎨' },
  'Estructural':   { bg: 'linear-gradient(135deg,#1a1a00 0%,#0d0d00 100%)', color: '#EAB308', icon: '🏗️' },
};
const DEFAULT_META = { bg: 'linear-gradient(135deg,#1a1200 0%,#0d0900 100%)', color: '#F59E0B', icon: '📦' };

export default function ProductCard({ product }: Props) {
  const { items, addToCart } = useCart();
  const cartItem = items.find((i) => i.product.id === product.id);
  const inCart = cartItem?.quantity ?? 0;
  const outOfStock = product.stock === 0;
  const lowStock = product.stock > 0 && product.stock <= 5;

  const meta = CATEGORY_META[product.category] ?? DEFAULT_META;

  return (
    <motion.div
      variants={cardVariants}
      whileHover={!outOfStock ? { y: -4, boxShadow: '0 12px 40px rgba(0,0,0,0.6), 0 0 0 1px rgba(245,158,11,0.2)' } : undefined}
      transition={{ type: 'spring', stiffness: 380, damping: 28 }}
      className="flex flex-col rounded-2xl overflow-hidden"
      style={{ background: '#18181F', border: '1px solid #2A2A38' }}
    >
      {/* Visual header */}
      <div className="relative h-28 overflow-hidden flex items-center justify-center" style={{ background: meta.bg }}>
        {/* Giant letter background */}
        <span
          className="absolute text-[6rem] font-black select-none leading-none"
          style={{ color: meta.color, opacity: 0.08, fontFamily: 'Barlow Condensed, sans-serif', bottom: '-0.5rem' }}
        >
          {product.category.charAt(0)}
        </span>

        {/* Emoji icon */}
        <span className="text-3xl relative z-10 drop-shadow-lg">{meta.icon}</span>

        {/* Category badge */}
        <div className="absolute top-2.5 left-3">
          <span
            className="text-xs font-semibold px-2 py-0.5 rounded-full"
            style={{
              background: 'rgba(0,0,0,0.5)',
              color: meta.color,
              border: `1px solid ${meta.color}33`,
              backdropFilter: 'blur(4px)',
            }}
          >
            {product.category}
          </span>
        </div>

        {/* Stock badge */}
        {(outOfStock || lowStock) && (
          <div className="absolute top-2.5 right-3">
            <span
              className="text-xs font-bold px-2 py-0.5 rounded-full"
              style={{
                background: outOfStock ? 'rgba(239,68,68,0.25)' : 'rgba(245,158,11,0.25)',
                color: outOfStock ? '#FCA5A5' : '#FCD34D',
                border: `1px solid ${outOfStock ? 'rgba(239,68,68,0.4)' : 'rgba(245,158,11,0.4)'}`,
                backdropFilter: 'blur(4px)',
              }}
            >
              {outOfStock ? 'Sin stock' : `¡Solo ${product.stock}!`}
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div className="flex flex-col flex-1 px-5 pt-5 pb-3">
        <h3 className="text-sm font-semibold leading-snug line-clamp-2 mb-2.5" style={{ color: '#F1F5F9' }}>
          {product.name}
        </h3>
        <p className="text-xs leading-loose line-clamp-2 flex-1" style={{ color: '#475569' }}>
          {product.description}
        </p>

        {/* Price row */}
        <div className="flex items-end justify-between mt-5 pt-4" style={{ borderTop: '1px solid #202029' }}>
          <div>
            <span className="text-xl font-bold" style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F59E0B' }}>
              ${product.price.toFixed(2)}
            </span>
            <span className="text-xs ml-1" style={{ color: '#334155' }}>/ {product.unit}</span>
          </div>
          {!outOfStock && !lowStock && (
            <span className="text-xs" style={{ color: '#334155' }}>
              {product.stock} {product.unit}
            </span>
          )}
        </div>
      </div>

      {/* CTA */}
      <div className="px-5 pb-6 pt-2">
        <motion.button
          disabled={outOfStock}
          onClick={() => addToCart(product)}
          whileTap={!outOfStock ? { scale: 0.96 } : undefined}
          className="w-full py-3 rounded-xl text-sm font-semibold transition-all duration-150"
          style={
            outOfStock
              ? { background: '#1E1E26', color: '#334155', cursor: 'not-allowed' }
              : inCart > 0
              ? { background: 'rgba(245,158,11,0.12)', color: '#F59E0B', border: '1px solid rgba(245,158,11,0.3)' }
              : { background: '#F59E0B', color: '#0C0C10' }
          }
        >
          {outOfStock
            ? 'Sin stock'
            : inCart > 0
            ? `✓ En carrito (${inCart}) — Agregar más`
            : 'Agregar al carrito'}
        </motion.button>
      </div>
    </motion.div>
  );
}

import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { getProducts, getCategories } from '../api/products';
import type { Product } from '../types';
import ProductCard, { cardVariants } from '../components/ProductCard';
import Navbar from '../components/Navbar';
import PageWrapper from '../components/PageWrapper';

const gridVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.055 } },
};

function SkeletonCard() {
  return (
    <div className="rounded-2xl overflow-hidden" style={{ background: '#18181F', border: '1px solid #2A2A38' }}>
      <div className="h-28 shimmer" />
      <div className="px-5 pt-5 pb-3 space-y-2.5">
        <div className="shimmer h-4 w-3/4 rounded" />
        <div className="shimmer h-3 w-full rounded" />
        <div className="shimmer h-3 w-5/6 rounded" />
        <div className="flex justify-between items-center pt-4 mt-4" style={{ borderTop: '1px solid #202029' }}>
          <div className="shimmer h-6 w-20 rounded" />
          <div className="shimmer h-3 w-12 rounded" />
        </div>
      </div>
      <div className="px-5 pb-6 pt-2">
        <div className="shimmer h-10 w-full rounded-xl" />
      </div>
    </div>
  );
}

export default function CatalogPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCategories().then(setCategories).catch(() => {});
  }, []);

  useEffect(() => {
    setLoading(true);
    getProducts({ search: search || undefined, category: category || undefined })
      .then(setProducts)
      .catch(() => setProducts([]))
      .finally(() => setLoading(false));
  }, [search, category]);

  const hasFilter = !!(search || category);

  return (
    <PageWrapper className="min-h-screen" style={{ background: '#0C0C10' }}>
      <Navbar />

      <main className="w-full">
        <div className="max-w-5xl mx-auto px-8 sm:px-12 lg:px-16 py-10 sm:py-14">

        {/* ── Hero header ─────────────────────────────────────────────── */}
        <div
          className="relative mb-12 sm:mb-16 rounded-2xl overflow-hidden"
          style={{ background: 'linear-gradient(135deg, #111116 0%, #14120a 100%)', border: '1px solid #2A2A38' }}
        >
          {/* Grid background */}
          <svg className="absolute inset-0 w-full h-full opacity-[0.025] pointer-events-none" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <pattern id="cat-grid" width="40" height="40" patternUnits="userSpaceOnUse">
                <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#F59E0B" strokeWidth="0.5" />
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#cat-grid)" />
          </svg>

          {/* Glow orbs */}
          <div className="absolute top-0 right-0 w-96 h-96 pointer-events-none"
            style={{ background: 'radial-gradient(circle, rgba(245,158,11,0.12) 0%, transparent 65%)', transform: 'translate(30%, -30%)' }}
          />
          <div className="absolute bottom-0 left-0 w-64 h-64 pointer-events-none"
            style={{ background: 'radial-gradient(circle, rgba(245,158,11,0.05) 0%, transparent 70%)', transform: 'translate(-30%, 30%)' }}
          />

          <div className="relative z-10 px-8 sm:px-12 py-12 sm:py-16 text-center">

            {/* Title row */}
            <div className="mb-8">
              <p className="text-xs font-semibold uppercase tracking-widest mb-4" style={{ color: '#F59E0B' }}>
                Firmeza · Inventario
              </p>
              <h1
                className="font-extrabold leading-none mb-4"
                style={{
                  fontFamily: 'Barlow Condensed, sans-serif',
                  fontSize: 'clamp(2.8rem, 6vw, 4.5rem)',
                  color: '#F8FAFC',
                  letterSpacing: '-0.01em',
                }}
              >
                CATÁLOGO
              </h1>
              <p className="text-sm sm:text-base max-w-md mx-auto" style={{ color: '#475569' }}>
                Materiales de construcción y maquinaria disponibles
              </p>
            </div>

            {/* Stats row */}
            {!loading && (
              <motion.div
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 }}
                className="flex justify-center gap-10 mb-8 pb-8"
                style={{ borderBottom: '1px solid #2A2A38' }}
              >
                {[
                  { num: products.length, label: 'productos' },
                  { num: categories.length, label: 'categorías' },
                ].map(({ num, label }) => (
                  <div key={label} className="flex items-baseline gap-2">
                    <span
                      className="text-3xl font-bold"
                      style={{ fontFamily: 'Barlow Condensed, sans-serif', color: '#F59E0B' }}
                    >
                      {num}
                    </span>
                    <span className="text-sm" style={{ color: '#475569' }}>{label}</span>
                  </div>
                ))}
              </motion.div>
            )}

            {/* Filters */}
            <div className="flex flex-col sm:flex-row gap-3 max-w-xl mx-auto">
              <div className="relative flex-1">
                <svg
                  className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 pointer-events-none"
                  style={{ color: '#475569' }}
                  fill="none" viewBox="0 0 24 24" stroke="currentColor"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0" />
                </svg>
                <input
                  type="text"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Buscar productos..."
                  className="input-dark pl-10"
                  style={{ background: 'rgba(12,12,16,0.6)', backdropFilter: 'blur(6px)' }}
                />
              </div>

              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="input-dark sm:w-52"
                style={{ cursor: 'pointer', background: 'rgba(12,12,16,0.6)', backdropFilter: 'blur(6px)' }}
              >
                <option value="">Todas las categorías</option>
                {categories.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>

              {hasFilter && (
                <motion.button
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  onClick={() => { setSearch(''); setCategory(''); }}
                  className="btn-ghost text-sm px-4 whitespace-nowrap"
                >
                  ✕ Limpiar
                </motion.button>
              )}
            </div>
          </div>
        </div>

        {/* ── Results count ─────────────────────────────────────────────── */}
        {!loading && hasFilter && (
          <motion.p
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-xs mb-5"
            style={{ color: '#334155' }}
          >
            {products.length} resultado{products.length !== 1 ? 's' : ''}
            {category ? ` en "${category}"` : ''}
            {search ? ` para "${search}"` : ''}
          </motion.p>
        )}

        {/* ── Grid ──────────────────────────────────────────────────────── */}
        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 sm:gap-8">
            {Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)}
          </div>
        ) : products.length === 0 ? (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-24 rounded-2xl"
            style={{ background: '#111116', border: '1px solid #2A2A38' }}
          >
            <div
              className="w-16 h-16 rounded-2xl flex items-center justify-center mx-auto mb-4"
              style={{ background: '#18181F', border: '1px solid #2A2A38' }}
            >
              <svg className="w-7 h-7" style={{ color: '#3E3E58' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
            </div>
            <p className="font-semibold mb-1" style={{ color: '#475569' }}>Sin resultados</p>
            <p className="text-sm mb-4" style={{ color: '#334155' }}>
              {hasFilter ? 'Prueba con otro filtro o búsqueda' : 'No hay productos disponibles'}
            </p>
            {hasFilter && (
              <button
                onClick={() => { setSearch(''); setCategory(''); }}
                className="text-sm font-semibold"
                style={{ color: '#F59E0B' }}
              >
                Ver todos los productos →
              </button>
            )}
          </motion.div>
        ) : (
          <motion.div
            key={`${search}-${category}`}
            variants={gridVariants}
            initial="hidden"
            animate="visible"
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 sm:gap-8"
          >
            {products.map((p) => (
              <motion.div key={p.id} variants={cardVariants}>
                <ProductCard product={p} />
              </motion.div>
            ))}
          </motion.div>
        )}
        </div>
      </main>
    </PageWrapper>
  );
}

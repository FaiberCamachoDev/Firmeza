import { useEffect, useState } from 'react';
import { getProducts, getCategories } from '../api/products';
import type { Product } from '../types';
import ProductCard from '../components/ProductCard';
import Navbar from '../components/Navbar';

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

  return (
    <div className="min-h-screen bg-slate-50">
      <Navbar />

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-slate-800">Catálogo de Productos</h1>
          <p className="text-slate-500 text-sm mt-1">Materiales de construcción y maquinaria disponibles</p>
        </div>

        {/* Filtros */}
        <div className="flex flex-col sm:flex-row gap-3 mb-8">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Buscar productos..."
            className="flex-1 border border-slate-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className="border border-slate-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
          >
            <option value="">Todas las categorías</option>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
        </div>

        {loading ? (
          <div className="text-center py-16 text-slate-400">Cargando productos...</div>
        ) : products.length === 0 ? (
          <div className="text-center py-16 text-slate-400">No se encontraron productos.</div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
            {products.map((p) => <ProductCard key={p.id} product={p} />)}
          </div>
        )}
      </main>
    </div>
  );
}

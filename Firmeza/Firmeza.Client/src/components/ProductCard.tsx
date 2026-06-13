import type { Product } from '../types';
import { useCart } from '../context/CartContext';

interface Props {
  product: Product;
}

export default function ProductCard({ product }: Props) {
  const { items, addToCart } = useCart();
  const cartItem = items.find((i) => i.product.id === product.id);
  const inCart = cartItem?.quantity ?? 0;
  const outOfStock = product.stock === 0;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-5 flex flex-col gap-3 hover:shadow-md transition">
      <div className="flex items-start justify-between gap-2">
        <div>
          <span className="text-xs font-semibold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded-full">
            {product.category}
          </span>
          <h3 className="mt-2 text-base font-semibold text-slate-800 leading-tight">{product.name}</h3>
        </div>
        <p className="text-lg font-bold text-indigo-700 whitespace-nowrap">
          ${product.price.toFixed(2)}
        </p>
      </div>

      <p className="text-sm text-slate-500 line-clamp-2">{product.description}</p>

      <div className="flex items-center justify-between mt-auto pt-2 border-t border-slate-50">
        <span className={`text-xs font-medium ${outOfStock ? 'text-red-500' : 'text-slate-400'}`}>
          {outOfStock ? 'Sin stock' : `Stock: ${product.stock} ${product.unit}`}
        </span>

        <button
          disabled={outOfStock}
          onClick={() => addToCart(product)}
          className="text-sm font-medium px-4 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:bg-slate-200 disabled:text-slate-400 disabled:cursor-not-allowed transition"
        >
          {inCart > 0 ? `Agregar (+${inCart})` : 'Agregar'}
        </button>
      </div>
    </div>
  );
}

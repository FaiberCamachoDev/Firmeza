import { createContext, useContext, useReducer, useCallback, type ReactNode } from 'react';
import type { CartItem, Product } from '../types';

interface CartState {
  items: CartItem[];
}

type CartAction =
  | { type: 'ADD'; product: Product }
  | { type: 'REMOVE'; productId: number }
  | { type: 'UPDATE_QTY'; productId: number; quantity: number }
  | { type: 'CLEAR' };

function cartReducer(state: CartState, action: CartAction): CartState {
  switch (action.type) {
    case 'ADD': {
      const existing = state.items.find((i) => i.product.id === action.product.id);
      if (existing) {
        return {
          items: state.items.map((i) =>
            i.product.id === action.product.id
              ? { ...i, quantity: Math.min(i.quantity + 1, action.product.stock) }
              : i
          ),
        };
      }
      return { items: [...state.items, { product: action.product, quantity: 1 }] };
    }
    case 'REMOVE':
      return { items: state.items.filter((i) => i.product.id !== action.productId) };
    case 'UPDATE_QTY':
      return {
        items: state.items.map((i) =>
          i.product.id === action.productId ? { ...i, quantity: action.quantity } : i
        ),
      };
    case 'CLEAR':
      return { items: [] };
    default:
      return state;
  }
}

interface CartContextValue {
  items: CartItem[];
  totalItems: number;
  subtotal: number;
  tax: number;
  total: number;
  addToCart: (product: Product) => void;
  removeFromCart: (productId: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  clearCart: () => void;
}

const CartContext = createContext<CartContextValue | null>(null);

const TAX_RATE = 0.12;

export function CartProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(cartReducer, { items: [] });

  const addToCart = useCallback((product: Product) => dispatch({ type: 'ADD', product }), []);
  const removeFromCart = useCallback((productId: number) => dispatch({ type: 'REMOVE', productId }), []);
  const updateQuantity = useCallback((productId: number, quantity: number) =>
    dispatch({ type: 'UPDATE_QTY', productId, quantity }), []);
  const clearCart = useCallback(() => dispatch({ type: 'CLEAR' }), []);

  const subtotal = state.items.reduce((acc, i) => acc + i.product.price * i.quantity, 0);
  const tax = subtotal * TAX_RATE;
  const total = subtotal + tax;
  const totalItems = state.items.reduce((acc, i) => acc + i.quantity, 0);

  return (
    <CartContext.Provider value={{
      items: state.items,
      totalItems,
      subtotal,
      tax,
      total,
      addToCart,
      removeFromCart,
      updateQuantity,
      clearCart,
    }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used inside CartProvider');
  return ctx;
}

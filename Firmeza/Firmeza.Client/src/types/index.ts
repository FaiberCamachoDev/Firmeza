export interface TokenResponse {
  token: string;
  expiresAt: string;
  email: string;
  fullName: string;
  roles: string[];
  customerId: number | null;
}

export interface AuthUser {
  token: string;
  expiresAt: string;
  email: string;
  fullName: string;
  roles: string[];
  customerId: number | null;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  unit: string;
  price: number;
  stock: number;
  isActive: boolean;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface SaleCreateDto {
  customerId: number;
  notes: string;
  items: { productId: number; quantity: number }[];
}

export interface SaleDetailDto {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface SaleDto {
  id: number;
  customerId: number;
  customerName: string;
  createdAt: string;
  total: number;
  notes: string;
  details: SaleDetailDto[];
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  documentNumber: string;
  phone: string;
}

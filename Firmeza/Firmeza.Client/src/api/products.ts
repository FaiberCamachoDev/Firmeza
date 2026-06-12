import apiClient from './client';
import type { Product } from '../types';

export const getProducts = (params?: { search?: string; category?: string }) =>
  apiClient.get<Product[]>('/api/products', { params: { active: true, ...params } }).then((r) => r.data);

export const getCategories = () =>
  apiClient.get<string[]>('/api/products/categories').then((r) => r.data);

import apiClient from './client';
import type { SaleCreateDto, SaleDto } from '../types';

export const createSale = (dto: SaleCreateDto) =>
  apiClient.post<SaleDto>('/api/sales', dto).then((r) => r.data);

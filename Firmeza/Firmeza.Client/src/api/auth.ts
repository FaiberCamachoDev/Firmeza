import apiClient from './client';
import type { LoginDto, RegisterDto, TokenResponse } from '../types';

export const login = (dto: LoginDto) =>
  apiClient.post<TokenResponse>('/api/auth/login', dto).then((r) => r.data);

export const register = (dto: RegisterDto) =>
  apiClient.post<TokenResponse>('/api/auth/register', dto).then((r) => r.data);

import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import type { AuthUser, TokenResponse } from '../types';
import apiClient from '../api/client';

// H5: sessionStorage en vez de localStorage — se limpia al cerrar la pestaña
// El token real NO se almacena aquí; viaja en cookie httpOnly gestionada por el browser
const STORAGE_KEY = 'firmeza_auth';

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  saveAuth: (data: TokenResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function loadFromStorage(): AuthUser | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const data: AuthUser = JSON.parse(raw);
    if (new Date(data.expiresAt) < new Date()) {
      sessionStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return data;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(loadFromStorage);

  const saveAuth = useCallback((data: TokenResponse) => {
    // Extraer solo metadata — el token es omitido intencionalmente
    const { token: _, ...metadata } = data;
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(metadata));
    setUser(metadata);
  }, []);

  const logout = useCallback(() => {
    // Limpiar la cookie httpOnly en el servidor, luego estado local
    void apiClient.post('/api/auth/logout').catch(() => {});
    sessionStorage.removeItem(STORAGE_KEY);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: user !== null, saveAuth, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}

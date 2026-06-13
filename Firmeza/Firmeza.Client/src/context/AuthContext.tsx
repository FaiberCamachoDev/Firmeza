import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import type { AuthUser, TokenResponse } from '../types';

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
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const data: AuthUser = JSON.parse(raw);
    if (new Date(data.expiresAt) < new Date()) {
      localStorage.removeItem(STORAGE_KEY);
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
    const auth: AuthUser = { ...data };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
    setUser(auth);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY);
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

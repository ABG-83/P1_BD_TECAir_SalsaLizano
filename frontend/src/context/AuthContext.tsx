import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { AuthUser } from '../types';

interface AuthContextValue {
  user: AuthUser | null;
  login: (user: AuthUser) => void;
  logout: () => void;
  isAuthenticated: boolean;
  isStaff: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const STORAGE_KEY = 'tecair_user';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });

  useEffect(() => {
    if (user) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, [user]);

  const login = (userData: AuthUser) => setUser(userData);
  const logout = () => setUser(null);

  return (
    <AuthContext.Provider value={{
      user,
      login,
      logout,
      isAuthenticated: !!user,
      isStaff: user?.rol === 'funcionario' || user?.rol === 'administrador',
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { Platform } from 'react-native';
import * as SQLite from 'expo-sqlite';
import type { AuthUser } from '../services/authService';

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (user: AuthUser) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  isAuthenticated: false,
  login: () => {},
  logout: () => {},
});

let sessionDb: SQLite.SQLiteDatabase | null = null;

const getSessionDb = (): SQLite.SQLiteDatabase | null => {
  if (Platform.OS === 'web') return null;
  if (!sessionDb) {
    sessionDb = SQLite.openDatabaseSync('tecair_session.db');
    sessionDb.execSync(`CREATE TABLE IF NOT EXISTS session (key TEXT PRIMARY KEY, value TEXT);`);
  }
  return sessionDb;
};

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<AuthUser | null>(null);

  useEffect(() => {
    const db = getSessionDb();
    if (!db) return;
    try {
      const row = db.getFirstSync('SELECT value FROM session WHERE key = ?', ['user']) as { value: string } | null;
      if (row?.value) setUser(JSON.parse(row.value));
    } catch {}
  }, []);

  const login = useCallback((u: AuthUser) => {
    setUser(u);
    const db = getSessionDb();
    if (!db) return;
    try {
      db.runSync(
        'INSERT OR REPLACE INTO session (key, value) VALUES (?, ?)',
        ['user', JSON.stringify(u)]
      );
    } catch {}
  }, []);

  const logout = useCallback(() => {
    setUser(null);
    const db = getSessionDb();
    if (!db) return;
    try {
      db.runSync('DELETE FROM session WHERE key = ?', ['user']);
    } catch {}
  }, []);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

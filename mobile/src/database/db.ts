import { Platform } from 'react-native';
import * as SQLite from 'expo-sqlite';
import * as Crypto from 'expo-crypto';

// Variable global para la base de datos nativa
let db: SQLite.SQLiteDatabase | null = null;

// Memoria en caché para simular la BD si el usuario prueba en la web
let webMockReservations: any[] = [];

/**
 * Inicializa la base de datos local y crea las tablas si no existen.
 */
export const initDatabase = () => {
  if (Platform.OS === 'web') {
    console.warn("⚠️ Ejecutando en Web: Simulando base de datos en memoria para propósitos de desarrollo visual.");
    return;
  }

  try {
    db = SQLite.openDatabaseSync('tecair.db');
    
    // Configuración recomendada para mejor rendimiento con SQLite
    db.execSync('PRAGMA journal_mode = WAL;');
    db.execSync('PRAGMA foreign_keys = ON;');

    db.execSync(`
      CREATE TABLE IF NOT EXISTS Usuarios (
        id INTEGER PRIMARY KEY,
        nombre TEXT NOT NULL,
        telefono TEXT,
        correo TEXT NOT NULL,
        es_estudiante INTEGER DEFAULT 0,
        carnet TEXT
      );

      CREATE TABLE IF NOT EXISTS Vuelos (
        id INTEGER PRIMARY KEY,
        origen TEXT NOT NULL,
        destino TEXT NOT NULL,
        precio REAL NOT NULL,
        fecha_salida TEXT,
        matricula_avion TEXT
      );

      CREATE TABLE IF NOT EXISTS Promociones (
        id INTEGER PRIMARY KEY,
        origen TEXT NOT NULL,
        destino TEXT NOT NULL,
        precio REAL NOT NULL,
        periodo TEXT,
        imagen TEXT
      );

      CREATE TABLE IF NOT EXISTS Reservaciones (
        id TEXT PRIMARY KEY,
        vuelo_id INTEGER NOT NULL,
        usuario_id INTEGER NOT NULL,
        asiento TEXT,
        maletas INTEGER DEFAULT 0,
        status_sync TEXT DEFAULT 'pending',
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (vuelo_id) REFERENCES Vuelos (id),
        FOREIGN KEY (usuario_id) REFERENCES Usuarios (id)
      );
    `);
    
    console.log("Base de datos local y tablas (Usuarios, Vuelos, Promociones, Reservaciones) inicializadas.");
  } catch (error) {
    console.error("Error inicializando la base de datos:", error);
  }
};

// ==========================================
// OPERACIONES PARA RESERVACIONES (OFFLINE PUSH)
// ==========================================

export const addOfflineReservation = (vuelo_id: number, usuario_id: number, asiento: string, maletas: number) => {
  const uuid = Crypto.randomUUID(); // <-- Inyección de UUID para evitar conflictos en Sync
  
  if (Platform.OS === 'web') {
    webMockReservations.unshift({
      id: uuid, vuelo_id, usuario_id, asiento, maletas, status_sync: 'pending', createdAt: new Date().toISOString()
    });
    return uuid;
  }

  if (!db) return null;
  db.runSync(
    'INSERT INTO Reservaciones (id, vuelo_id, usuario_id, asiento, maletas, status_sync) VALUES (?, ?, ?, ?, ?, ?)',
    [uuid, vuelo_id, usuario_id, asiento, maletas, 'pending']
  );
  return uuid;
};

export const getPendingReservations = () => {
  if (Platform.OS === 'web') return webMockReservations.filter(r => r.status_sync === 'pending');
  if (!db) return [];
  return db.getAllSync('SELECT * FROM Reservaciones WHERE status_sync = ?', ['pending']);
};

export const updateReservationSyncStatus = (id: string, newStatus: 'synced' | 'failed_conflict') => {
  if (Platform.OS === 'web') {
    const res = webMockReservations.find(r => r.id === id);
    if (res) res.status_sync = newStatus;
    return;
  }
  
  if (!db) return;
  db.runSync('UPDATE Reservaciones SET status_sync = ? WHERE id = ?', [newStatus, id]);
};

// ==========================================
// OPERACIONES PARA SINCRONIZACIÓN (PULL)
// ==========================================

export const syncVuelosLocales = (vuelos: any[]) => {
  if (Platform.OS === 'web') return; 
  if (!db) return;
  
  db.execSync('DELETE FROM Vuelos');
  for (const v of vuelos) {
    db.runSync(
      'INSERT INTO Vuelos (id, origen, destino, precio, fecha_salida, matricula_avion) VALUES (?, ?, ?, ?, ?, ?)',
      [v.id, v.origen, v.destino, v.precio, v.fecha_salida, v.matricula_avion]
    );
  }
};

export const syncPromocionesLocales = (promociones: any[]) => {
  if (Platform.OS === 'web') return;
  if (!db) return;
  
  db.execSync('DELETE FROM Promociones');
  for (const p of promociones) {
    db.runSync(
      'INSERT INTO Promociones (id, origen, destino, precio, periodo, imagen) VALUES (?, ?, ?, ?, ?, ?)',
      [p.id, p.origen, p.destino, p.precio, p.periodo, p.imagen]
    );
  }
};

export default db;

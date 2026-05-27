import { Platform } from 'react-native';
import * as SQLite from 'expo-sqlite';
import * as Crypto from 'expo-crypto';

// Variable global para la base de datos nativa (Inicializada al cargar el módulo)
let db: SQLite.SQLiteDatabase | null = null;
if (Platform.OS !== 'web') {
  try {
    db = SQLite.openDatabaseSync('tecair.db');
  } catch (error) {
    console.error("Error al abrir la base de datos SQLite:", error);
  }
}

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
    if (!db) {
      db = SQLite.openDatabaseSync('tecair.db');
    }
    
    // Configuración recomendada para mejor rendimiento con SQLite
    db.execSync('PRAGMA journal_mode = WAL;');
    db.execSync('PRAGMA foreign_keys = ON;');

    db.execSync(`
      CREATE TABLE IF NOT EXISTS Aeropuertos (
        id INTEGER PRIMARY KEY,
        nombre TEXT NOT NULL,
        ubicacion TEXT
      );

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
        codigo_vuelo TEXT,
        origen_id INTEGER,
        destino_id INTEGER,
        origen TEXT NOT NULL,
        destino TEXT NOT NULL,
        precio REAL NOT NULL,
        fecha_salida TEXT,
        fecha_llegada TEXT,
        matricula_avion TEXT,
        estado TEXT DEFAULT 'Scheduled'
      );

      CREATE TABLE IF NOT EXISTS Promociones (
        id INTEGER PRIMARY KEY,
        origen_id INTEGER,
        destino_id INTEGER,
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
        cantidad_asientos INTEGER DEFAULT 1,
        status_sync TEXT DEFAULT 'pending',
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (vuelo_id) REFERENCES Vuelos (id),
        FOREIGN KEY (usuario_id) REFERENCES Usuarios (id)
      );
    `);
    
    // Migraciones dinámicas para bases de datos existentes en el dispositivo
    const migrations = [
      'ALTER TABLE Vuelos ADD COLUMN codigo_vuelo TEXT;',
      'ALTER TABLE Vuelos ADD COLUMN origen_id INTEGER;',
      'ALTER TABLE Vuelos ADD COLUMN destino_id INTEGER;',
      'ALTER TABLE Vuelos ADD COLUMN fecha_llegada TEXT;',
      'ALTER TABLE Vuelos ADD COLUMN estado TEXT DEFAULT \'Scheduled\';',
      'ALTER TABLE Promociones ADD COLUMN origen_id INTEGER;',
      'ALTER TABLE Promociones ADD COLUMN destino_id INTEGER;',
      'ALTER TABLE Reservaciones ADD COLUMN cantidad_asientos INTEGER DEFAULT 1;',
    ];
    for (const m of migrations) {
      try { db.execSync(m); } catch { /* columna ya existe */ }
    }
    
    console.log("Base de datos local y tablas (Usuarios, Vuelos, Promociones, Reservaciones) inicializadas.");
  } catch (error) {
    console.error("Error inicializando la base de datos:", error);
  }
};

// ==========================================
// OPERACIONES PARA RESERVACIONES (OFFLINE PUSH)
// ==========================================

export const addOfflineReservation = (vuelo_id: number, usuario_id: number, asiento: string, maletas: number, cantidad_asientos: number = 1) => {
  const uuid = Crypto.randomUUID(); // <-- Inyección de UUID para evitar conflictos en Sync
  
  if (Platform.OS === 'web') {
    webMockReservations.unshift({
      id: uuid, vuelo_id, usuario_id, asiento, maletas, cantidad_asientos, status_sync: 'pending', createdAt: new Date().toISOString()
    });
    return uuid;
  }

  if (!db) return null;
  db.runSync(
    'INSERT INTO Reservaciones (id, vuelo_id, usuario_id, asiento, maletas, cantidad_asientos, status_sync) VALUES (?, ?, ?, ?, ?, ?, ?)',
    [uuid, vuelo_id, usuario_id, asiento, maletas, cantidad_asientos, 'pending']
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

const parseFlightNumber = (flightNumber?: string | number): number => {
  if (typeof flightNumber === 'number') {
    return Number.isFinite(flightNumber) ? flightNumber : 0;
  }
  if (!flightNumber) return 0;
  const digits = flightNumber.match(/\d+/g)?.join('');
  return digits ? Number(digits) : 0;
};

export const syncAeropuertosLocales = (aeropuertos: any[]) => {
  if (Platform.OS === 'web') return;
  if (!db) return;
  try {
    db.execSync('DELETE FROM Aeropuertos');
    for (const a of aeropuertos) {
      const id  = a.airportId ?? a.AirportId ?? 0;
      const nom = a.name      ?? a.Name      ?? '';
      const ub  = a.location  ?? a.Location  ?? '';
      if (!id) continue;
      db.runSync(
        'INSERT INTO Aeropuertos (id, nombre, ubicacion) VALUES (?, ?, ?)',
        [id, nom, ub]
      );
    }
    console.log(`Sincronización local de aeropuertos: ${aeropuertos.length} registros.`);
  } catch (error) {
    console.error("Error al sincronizar aeropuertos locales:", error);
  }
};

export const getAeropuertosLocales = (): { airportId: number; name: string; location: string }[] => {
  if (Platform.OS === 'web') return [];
  if (!db) return [];
  try {
    const rows = db.getAllSync('SELECT id as airportId, nombre as name, ubicacion as location FROM Aeropuertos ORDER BY nombre');
    return rows as { airportId: number; name: string; location: string }[];
  } catch {
    return [];
  }
};

export const syncVuelosLocales = (vuelos: any[]) => {
  if (Platform.OS === 'web') return;
  if (!db) return;
  try {
    db.execSync('DELETE FROM Vuelos');
    for (const v of vuelos) {
      const id = parseFlightNumber(v.flightNumber);
      const originId = v.origin?.airportId ?? v.originAirportId ?? 0;
      const destId = v.destination?.airportId ?? v.destinationAirportId ?? 0;
      const originName = v.origin?.name ?? '';
      const destName = v.destination?.name ?? '';
      db.runSync(
        'INSERT INTO Vuelos (id, codigo_vuelo, origen_id, destino_id, origen, destino, precio, fecha_salida, fecha_llegada, matricula_avion, estado) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)',
        [id, v.flightNumber || `TA-${id}`, originId, destId, originName, destName, v.price ?? 0, v.departureTime ?? '', v.arrivalTime ?? '', v.airplanePlateNumber ?? '', v.status ?? 'Scheduled']
      );
    }
    console.log(`Sincronización local de vuelos: ${vuelos.length} registros.`);
  } catch (error) {
    console.error("Error al sincronizar vuelos locales:", error);
  }
};

export const getVuelosLocales = (): any[] => {
  if (Platform.OS === 'web') return [];
  if (!db) return [];
  try {
    return db.getAllSync('SELECT * FROM Vuelos') as any[];
  } catch {
    return [];
  }
};

export const syncPromocionesLocales = (promociones: any[]) => {
  if (Platform.OS === 'web') return;
  if (!db) return;
  try {
    db.execSync('DELETE FROM Promociones');
    for (const p of promociones) {
      const originId   = p.origin?.airportId   ?? p.origin?.AirportId   ?? p.Origin?.airportId   ?? p.Origin?.AirportId   ?? p.originAirportId   ?? 0;
      const destId     = p.destination?.airportId ?? p.destination?.AirportId ?? p.Destination?.airportId ?? p.Destination?.AirportId ?? p.destinationAirportId ?? 0;
      const originName = p.origin?.name   ?? p.origin?.Name   ?? p.Origin?.name   ?? p.Origin?.Name   ?? '';
      const destName   = p.destination?.name ?? p.destination?.Name ?? p.Destination?.name ?? p.Destination?.Name ?? '';
      const startRaw   = String(p.startDate ?? p.StartDate ?? '');
      const endRaw     = String(p.endDate   ?? p.EndDate   ?? '');
      const period     = startRaw && endRaw
        ? `${startRaw.split('T')[0]} a ${endRaw.split('T')[0]}`
        : (p.periodo || 'Promoción Activa');
      const id         = p.promotionId ?? p.PromotionId ?? p.id ?? 0;
      db.runSync(
        'INSERT INTO Promociones (id, origen_id, destino_id, origen, destino, precio, periodo, imagen) VALUES (?, ?, ?, ?, ?, ?, ?, ?)',
        [id, originId, destId, originName, destName, Number(p.price ?? p.Price ?? p.precio ?? 0), period, p.image ?? p.Image ?? p.imagen ?? '']
      );
    }
    console.log(`Sincronización local de promociones: ${promociones.length} registros.`);
  } catch (error) {
    console.error("Error al sincronizar promociones locales:", error);
  }
};

export const getPromocionesLocales = (): import('../services/promotionService').Promotion[] => {
  if (Platform.OS === 'web') return [];
  if (!db) return [];
  try {
    const rows = db.getAllSync('SELECT * FROM Promociones') as any[];
    return rows.map(p => ({
      promotionId: p.id,
      price: p.precio,
      startDate: p.periodo?.split(' a ')[0] ?? '',
      endDate:   p.periodo?.split(' a ')[1] ?? '',
      image: p.imagen || undefined,
      isActive: true,
      origin:      { airportId: p.origen_id ?? 0,  name: p.origen ?? '',  location: '' },
      destination: { airportId: p.destino_id ?? 0, name: p.destino ?? '', location: '' },
    }));
  } catch {
    return [];
  }
};

export const getFlightNumberById = (id: number): string | null => {
  if (Platform.OS === 'web') {
    return id === 1 ? 'TA-001' : 'TA-002';
  }
  if (!db) return null;
  try {
    const row = db.getFirstSync('SELECT codigo_vuelo FROM Vuelos WHERE id = ?', [id]) as { codigo_vuelo: string } | null;
    return row ? row.codigo_vuelo : null;
  } catch (error) {
    console.error("Error obteniendo código de vuelo:", error);
    return null;
  }
};

export const getAllReservations = () => {
  if (Platform.OS === 'web') {
    return webMockReservations.map(r => ({
      id: r.id,
      flightId: String(r.vuelo_id),
      userId: String(r.usuario_id),
      status: r.status_sync,
      createdAt: r.createdAt
    }));
  }
  if (!db) return [];
  try {
    const rows = db.getAllSync('SELECT id, vuelo_id as flightId, usuario_id as userId, status_sync as status, createdAt FROM Reservaciones ORDER BY createdAt DESC');
    return rows.map((r: any) => ({
      ...r,
      flightId: String(r.flightId),
      userId: String(r.userId),
    }));
  } catch (error) {
    console.error("Error obteniendo reservaciones locales:", error);
    return [];
  }
};

export default db;

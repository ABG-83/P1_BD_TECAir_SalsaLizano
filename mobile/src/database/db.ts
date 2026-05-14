import * as SQLite from 'expo-sqlite';

// Inicializar y abrir la base de datos de manera sincrónica (Expo SQLite v14+)
const db = SQLite.openDatabaseSync('TECAir.db');

export const initDatabase = () => {
  try {
    // Tabla para almacenar reservaciones creadas de manera offline
    db.execSync(`
      CREATE TABLE IF NOT EXISTS Reservations (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        flightId TEXT NOT NULL,
        userId TEXT NOT NULL,
        status TEXT DEFAULT 'pending_sync', -- 'pending_sync' or 'synced'
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
      );
    `);

    console.log("Base de datos local inicializada correctamente.");
  } catch (error) {
    console.error("Error inicializando la base de datos:", error);
  }
};

export const getPendingReservations = () => {
  return db.getAllSync('SELECT * FROM Reservations WHERE status = ?', ['pending_sync']);
};

export const markReservationAsSynced = (id: number) => {
  db.runSync('UPDATE Reservations SET status = ? WHERE id = ?', ['synced', id]);
};

export const addOfflineReservation = (flightId: string, userId: string) => {
  const result = db.runSync(
    'INSERT INTO Reservations (flightId, userId, status) VALUES (?, ?, ?)',
    [flightId, userId, 'pending_sync']
  );
  return result.lastInsertRowId;
};

export default db;

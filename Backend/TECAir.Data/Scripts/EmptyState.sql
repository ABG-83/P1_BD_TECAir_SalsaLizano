
--  TECAir — Empty State (Creación de Base de Datos)
--   Script de creación de estructura sin datos.


-- Eliminar tablas si ya existen (en orden por dependencias FK)
DROP TABLE IF EXISTS VUELO_ESCALA    CASCADE;
DROP TABLE IF EXISTS PASE_ABORDAJE   CASCADE;
DROP TABLE IF EXISTS MALETA          CASCADE;
DROP TABLE IF EXISTS RESERVACION     CASCADE;
DROP TABLE IF EXISTS PROMOCION       CASCADE;
DROP TABLE IF EXISTS VUELO           CASCADE;
DROP TABLE IF EXISTS AEROPUERTO      CASCADE;
DROP TABLE IF EXISTS AVION           CASCADE;
DROP TABLE IF EXISTS USUARIO         CASCADE;


-- TABLA: USUARIO

CREATE TABLE USUARIO (
    ID_Usuario    SERIAL          PRIMARY KEY,
    Nombre        VARCHAR(150)    NOT NULL,
    Correo        VARCHAR(150)    NOT NULL UNIQUE,
    Telefono      VARCHAR(20)     NOT NULL,
    Rol           VARCHAR(30)     NOT NULL
                                  CHECK (Rol IN ('cliente', 'funcionario', 'administrador')),
    Millas        INT             NOT NULL DEFAULT 0
                                  CHECK (Millas >= 0),
    Carnet        VARCHAR(50),
    Universidad   VARCHAR(150)
);

COMMENT ON TABLE  USUARIO              IS 'Usuarios del sistema: clientes y funcionarios del aeropuerto.';
COMMENT ON COLUMN USUARIO.Carnet       IS 'Carnet universitario. Solo aplica si el usuario es estudiante.';
COMMENT ON COLUMN USUARIO.Universidad  IS 'Universidad del estudiante. Solo aplica si el usuario es estudiante.';


-- TABLA: AVION

CREATE TABLE AVION (
    Matricula       VARCHAR(20)     PRIMARY KEY,
    Cap_Pasajeros   INT             NOT NULL CHECK (Cap_Pasajeros > 0),
    Num_Asientos    INT             NOT NULL CHECK (Num_Asientos > 0)
);

COMMENT ON TABLE AVION IS 'Aviones disponibles de la aerolínea TECAir.';


-- TABLA: AEROPUERTO

CREATE TABLE AEROPUERTO (
    ID_Aeropuerto   SERIAL          PRIMARY KEY,
    Nombre          VARCHAR(150)    NOT NULL,
    Ubicacion       VARCHAR(200)    NOT NULL
);

COMMENT ON TABLE AEROPUERTO IS 'Aeropuertos de origen, destino y escala de los vuelos.';


-- TABLA: VUELO

CREATE TABLE VUELO (
    Num_Vuelo               SERIAL          PRIMARY KEY,
    Hora_Salida             TIMESTAMP       NOT NULL,
    Hora_Llegada            TIMESTAMP       NOT NULL,
    Estado                  VARCHAR(20)     NOT NULL DEFAULT 'programado'
                                            CHECK (Estado IN ('programado', 'abierto', 'cerrado', 'cancelado')),
    Matricula               VARCHAR(20)     NOT NULL,
    ID_Aeropuerto_Origen    INT             NOT NULL,
    ID_Aeropuerto_Destino   INT             NOT NULL,

    CONSTRAINT fk_vuelo_avion
        FOREIGN KEY (Matricula)
        REFERENCES AVION(Matricula)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_vuelo_origen
        FOREIGN KEY (ID_Aeropuerto_Origen)
        REFERENCES AEROPUERTO(ID_Aeropuerto)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_vuelo_destino
        FOREIGN KEY (ID_Aeropuerto_Destino)
        REFERENCES AEROPUERTO(ID_Aeropuerto)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT chk_vuelo_horas
        CHECK (Hora_Llegada > Hora_Salida),

    CONSTRAINT chk_vuelo_aeropuertos
        CHECK (ID_Aeropuerto_Origen <> ID_Aeropuerto_Destino)
);

COMMENT ON TABLE  VUELO               IS 'Vuelos registrados en TECAir con su ruta y avión asignado.';
COMMENT ON COLUMN VUELO.Estado        IS 'programado → abierto → cerrado. También puede ser cancelado.';
COMMENT ON COLUMN VUELO.Hora_Salida   IS 'Fecha y hora exacta de salida del vuelo.';
COMMENT ON COLUMN VUELO.Hora_Llegada  IS 'Fecha y hora estimada de llegada. Debe ser posterior a Hora_Salida.';


-- TABLA: VUELO_ESCALA

CREATE TABLE VUELO_ESCALA (
    Num_Vuelo       INT     NOT NULL,
    ID_Aeropuerto   INT     NOT NULL,
    Orden_Escala    INT     NOT NULL CHECK (Orden_Escala >= 1),

    CONSTRAINT pk_vuelo_escala
        PRIMARY KEY (Num_Vuelo, ID_Aeropuerto),

    CONSTRAINT fk_escala_vuelo
        FOREIGN KEY (Num_Vuelo)
        REFERENCES VUELO(Num_Vuelo)
        ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT fk_escala_aeropuerto
        FOREIGN KEY (ID_Aeropuerto)
        REFERENCES AEROPUERTO(ID_Aeropuerto)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE  VUELO_ESCALA              IS 'Aeropuertos de escala de cada vuelo, con su orden en la ruta.';
COMMENT ON COLUMN VUELO_ESCALA.Orden_Escala IS 'Posición de la escala en la ruta: 1 = primera escala, 2 = segunda, etc.';

-- TABLA: RESERVACION

CREATE TABLE RESERVACION (
    Cod_Reservacion     SERIAL          PRIMARY KEY,
    Fecha               TIMESTAMP       NOT NULL DEFAULT NOW(),
    Estado_Pago         VARCHAR(20)     NOT NULL DEFAULT 'pendiente'
                                        CHECK (Estado_Pago IN ('pendiente', 'pagado', 'cancelado')),
    ID_Usuario          INT             NOT NULL,
    Num_Vuelo           INT             NOT NULL,

    CONSTRAINT fk_reservacion_usuario
        FOREIGN KEY (ID_Usuario)
        REFERENCES USUARIO(ID_Usuario)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_reservacion_vuelo
        FOREIGN KEY (Num_Vuelo)
        REFERENCES VUELO(Num_Vuelo)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE RESERVACION IS 'Reservaciones de pasajeros para vuelos específicos.';


-- TABLA: PASE_ABORDAJE

CREATE TABLE PASE_ABORDAJE (
    ID_Pase             SERIAL          PRIMARY KEY,
    Asiento             VARCHAR(10)     NOT NULL,
    Puerta_Abordaje     VARCHAR(10)     NOT NULL,
    Hora_Impresion      TIMESTAMP       NOT NULL DEFAULT NOW(),
    Cod_Reservacion     INT             NOT NULL UNIQUE,
    Num_Vuelo           INT             NOT NULL,

    CONSTRAINT fk_pase_reservacion
        FOREIGN KEY (Cod_Reservacion)
        REFERENCES RESERVACION(Cod_Reservacion)
        ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT fk_pase_vuelo
        FOREIGN KEY (Num_Vuelo)
        REFERENCES VUELO(Num_Vuelo)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE  PASE_ABORDAJE                  IS 'Pases de abordaje generados al hacer check-in de una reservación.';
COMMENT ON COLUMN PASE_ABORDAJE.Cod_Reservacion  IS 'UNIQUE garantiza la relación 1:1 con RESERVACION.';


-- TABLA: MALETA

CREATE TABLE MALETA (
    Num_Maleta          SERIAL          PRIMARY KEY,
    Peso                DECIMAL(6,2)    NOT NULL CHECK (Peso > 0),
    Color               VARCHAR(50)     NOT NULL,
    Cod_Reservacion     INT             NOT NULL,
    ID_Usuario          INT             NOT NULL,

    CONSTRAINT fk_maleta_reservacion
        FOREIGN KEY (Cod_Reservacion)
        REFERENCES RESERVACION(Cod_Reservacion)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_maleta_usuario
        FOREIGN KEY (ID_Usuario)
        REFERENCES USUARIO(ID_Usuario)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE MALETA IS 'Maletas asignadas a pasajeros ya chequeados. La primera es gratis, la 2da cuesta $50 y las siguientes $75 c/u.';

-- TABLA: PROMOCION

CREATE TABLE PROMOCION (
    ID_Promocion            SERIAL          PRIMARY KEY,
    Precio                  DECIMAL(10,2)   NOT NULL CHECK (Precio > 0),
    Fecha_Inicio            DATE            NOT NULL,
    Fecha_Fin               DATE            NOT NULL,
    Imagen                  VARCHAR(300),
    Estado_Activa           BOOLEAN         NOT NULL DEFAULT TRUE,
    ID_Aeropuerto_Origen    INT             NOT NULL,
    ID_Aeropuerto_Destino   INT             NOT NULL,

    CONSTRAINT fk_promo_origen
        FOREIGN KEY (ID_Aeropuerto_Origen)
        REFERENCES AEROPUERTO(ID_Aeropuerto)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_promo_destino
        FOREIGN KEY (ID_Aeropuerto_Destino)
        REFERENCES AEROPUERTO(ID_Aeropuerto)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT chk_promo_fechas
        CHECK (Fecha_Fin > Fecha_Inicio),

    CONSTRAINT chk_promo_aeropuertos
        CHECK (ID_Aeropuerto_Origen <> ID_Aeropuerto_Destino)
);

COMMENT ON TABLE  PROMOCION               IS 'Promociones de precio entre pares de aeropuertos con vigencia definida.';
COMMENT ON COLUMN PROMOCION.Imagen        IS 'Ruta o URL de la imagen de la promoción. Opcional.';
COMMENT ON COLUMN PROMOCION.Estado_Activa IS 'TRUE = promoción vigente y visible. FALSE = desactivada.';


-- ÍNDICES adicionales para mejorar rendimiento en consultas
-- frecuentes (búsqueda de vuelos, reservaciones por usuario)

CREATE INDEX idx_vuelo_salida   ON VUELO(Hora_Salida);
CREATE INDEX idx_vuelo_origen   ON VUELO(ID_Aeropuerto_Origen);
CREATE INDEX idx_vuelo_destino  ON VUELO(ID_Aeropuerto_Destino);
CREATE INDEX idx_res_usuario    ON RESERVACION(ID_Usuario);
CREATE INDEX idx_res_vuelo      ON RESERVACION(Num_Vuelo);
CREATE INDEX idx_maleta_res     ON MALETA(Cod_Reservacion);
CREATE INDEX idx_promo_activa   ON PROMOCION(Estado_Activa);


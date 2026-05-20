-- -----------------------------------------------------------
--  TECAir — Initial State (Populación de Base de Datos)
--  Descripción: Script de inserción de datos de prueba.
--               Ejecutar DESPUÉS del Empty State
-----------------------------------------------------------------


-- TABLA: USUARIO
-- Cubre: clientes normales, estudiantes con carnet/universidad,
--        funcionarios de aeropuerto y un administrador.

INSERT INTO USUARIO (Nombre, Correo, Telefono, Rol, Millas, Carnet, Universidad) VALUES
-- Estudiantes (tienen carnet y universidad)
('Carlos Mendoza Vargas',    'carlos.mendoza@estudiantec.cr',  '88001122', 'cliente', 1500, 'C-2021-0001', 'Instituto Tecnológico de Costa Rica'),
('Ana Sofía Jiménez Rojas',  'ana.jimenez@estudiantec.cr',     '88002233', 'cliente', 800,  'C-2022-0045', 'Instituto Tecnológico de Costa Rica'),
('Diego Hernández Mora',     'diego.hernandez@ucr.ac.cr',      '88003344', 'cliente', 200,  'B95432',      'Universidad de Costa Rica'),
('Valeria Torres Campos',    'valeria.torres@una.ac.cr',       '88004455', 'cliente', 3200, 'UNA-20190034','Universidad Nacional de Costa Rica'),
-- Clientes normales (sin carnet)
('Luis Fernando Quesada',    'luis.quesada@gmail.com',         '88005566', 'cliente', 500,  NULL, NULL),
('María Elena Castillo',     'maria.castillo@gmail.com',       '88006677', 'cliente', 950,  NULL, NULL),
('Roberto Alvarado Pérez',   'roberto.alvarado@hotmail.com',   '88007788', 'cliente', 100,  NULL, NULL),
('Daniela Núñez Solís',      'daniela.nunez@outlook.com',      '88008899', 'cliente', 2700, NULL, NULL),
('Andrés Calvo Blanco',      'andres.calvo@gmail.com',         '88009900', 'cliente', 0,    NULL, NULL),
-- Funcionarios del aeropuerto
('Patricia Solano Vega',     'patricia.solano@tecair.cr',      '22001100', 'funcionario', 0, NULL, NULL),
('Marco Rodríguez Fallas',   'marco.rodriguez@tecair.cr',      '22002200', 'funcionario', 0, NULL, NULL),
-- Administrador
('Admin TECAir',             'admin@tecair.cr',                 '22003300', 'administrador', 0, NULL, NULL);



-- TABLA: AVION

INSERT INTO AVION (Matricula, Cap_Pasajeros, Num_Asientos) VALUES
('TEC-001', 180, 180),
('TEC-002', 150, 150),
('TEC-003', 200, 200),
('TEC-004', 120, 120);



-- TABLA: AEROPUERTO

INSERT INTO AEROPUERTO (Nombre, Ubicacion) VALUES
('Aeropuerto Internacional Juan Santamaría',  'Alajuela, Costa Rica'),
('Aeropuerto Internacional Daniel Oduber',    'Liberia, Guanacaste, Costa Rica'),
('Aeropuerto Internacional El Dorado',        'Bogotá, Colombia'),
('Aeropuerto Internacional Jorge Chávez',     'Lima, Perú'),
('Aeropuerto Internacional Tocumen',          'Ciudad de Panamá, Panamá'),
('Aeropuerto Internacional Benito Juárez',    'Ciudad de México, México'),
('Aeropuerto Internacional Comairas',         'Buenos Aires, Argentina'),
('Aeropuerto de Miami',                       'Miami, Florida, Estados Unidos');



-- TABLA: VUELO
-- Cubre distintos estados: programado, abierto, crrado
-- Aeropuertos:
--   1 = Juan Santamaría (SJO)
--   2 = Daniel Oduber   (LIR)
--   3 = El Dorado       (BOG)
--   4 = Jorge Chávez    (LIM)
--   5 = Tocumen         (PTY)
--   6 = Benito Juárez   (MEX)
--   7 = Comairas        (EZE)
--   8 = Miami           (MIA)

INSERT INTO VUELO (Hora_Salida, Hora_Llegada, Estado, Matricula, ID_Aeropuerto_Origen, ID_Aeropuerto_Destino) VALUES
-- Vuelo 1: SJO → BOG | programado
('2026-06-01 06:00:00', '2026-06-01 09:30:00', 'programado', 'TEC-001', 1, 3),
-- Vuelo 2: SJO → LIM | programado
('2026-06-01 08:00:00', '2026-06-01 13:00:00', 'programado', 'TEC-002', 1, 4),
-- Vuelo 3: SJO → PTY | abierto (para demostrar check-in)
('2026-05-20 10:00:00', '2026-05-20 11:30:00', 'abierto',    'TEC-003', 1, 5),
-- Vuelo 4: SJO → MEX | abierto
('2026-05-20 14:00:00', '2026-05-20 17:45:00', 'abierto',    'TEC-004', 1, 6),
-- Vuelo 5: SJO → MIA | cerrado (para demostrar cierre de vuelo)
('2026-05-19 07:00:00', '2026-05-19 10:00:00', 'cerrado',    'TEC-001', 1, 8),
-- Vuelo 6: BOG → LIM | programado (escala en PTY)
('2026-06-02 09:00:00', '2026-06-02 15:00:00', 'programado', 'TEC-002', 3, 4),
-- Vuelo 7: LIR → MIA | programado
('2026-06-03 11:00:00', '2026-06-03 15:30:00', 'programado', 'TEC-003', 2, 8),
-- Vuelo 8: SJO → EZE | programado (con escalas)
('2026-06-05 05:00:00', '2026-06-05 20:00:00', 'programado', 'TEC-004', 1, 7);



-- TABLA: VUELO_ESCALA
-- Cubre rutas con 1 y 2 escalas intermedias

INSERT INTO VUELO_ESCALA (Num_Vuelo, ID_Aeropuerto, Orden_Escala) VALUES
-- Vuelo 6 (BOG → LIM): escala en PTY
(6, 5, 1),
-- Vuelo 8 (SJO → EZE): escala 1 en PTY, escala 2 en BOG
(8, 5, 1),
(8, 3, 2);



-- TABLA: RESERVACION
-- Cubre distintos estados de pago y vuelos

INSERT INTO RESERVACION (Fecha, Estado_Pago, ID_Usuario, Num_Vuelo) VALUES
-- Reservaciones en vuelo 3 (abierto — para check-in)
('2026-05-10 09:00:00', 'pagado',    1, 3),   -- Carlos   → vuelo 3
('2026-05-10 09:30:00', 'pagado',    2, 3),   -- Ana      → vuelo 3
('2026-05-11 10:00:00', 'pagado',    5, 3),   -- Luis     → vuelo 3
('2026-05-11 10:30:00', 'pagado',    6, 3),   -- María    → vuelo 3
('2026-05-12 11:00:00', 'pagado',    8, 3),   -- Daniela  → vuelo 3
-- Reservaciones en vuelo 4 (abierto)
('2026-05-13 08:00:00', 'pagado',    3, 4),   -- Diego    → vuelo 4
('2026-05-13 08:30:00', 'pagado',    4, 4),   -- Valeria  → vuelo 4
('2026-05-14 09:00:00', 'pendiente', 7, 4),   -- Roberto  → vuelo 4 (pendiente de pago)
-- Reservaciones en vuelo 5 (cerrado)
('2026-05-01 07:00:00', 'pagado',    1, 5),   -- Carlos   → vuelo 5
('2026-05-01 07:30:00', 'pagado',    5, 5),   -- Luis     → vuelo 5
-- Reservaciones en vuelos futuros
('2026-05-15 10:00:00', 'pagado',    9, 1),   -- Andrés   → vuelo 1
('2026-05-15 10:30:00', 'pagado',    2, 2),   -- Ana      → vuelo 2
('2026-05-16 11:00:00', 'cancelado', 6, 1);   -- María    → vuelo 1 (cancelada)



-- TABLA: PASE_ABORDAJE
-- Solo los pasajeros ya chequeados tienen pase.
-- Cubre vuelos abierto (3, 4) y cerrado (5).
-- Cod_Reservacion:
--   1 = Carlos/vuelo3   2 = Ana/vuelo3   3 = Luis/vuelo3
--   4 = María/vuelo3    5 = Daniela/vuelo3
--   6 = Diego/vuelo4    7 = Valeria/vuelo4

INSERT INTO PASE_ABORDAJE (Asiento, Puerta_Abordaje, Hora_Impresion, Cod_Reservacion, Num_Vuelo) VALUES
-- Check-in vuelo 3 (SJO→PTY, abierto)
('12A', 'B3', '2026-05-20 08:00:00', 1,  3),
('12B', 'B3', '2026-05-20 08:05:00', 2,  3),
('14C', 'B3', '2026-05-20 08:10:00', 3,  3),
('15A', 'B3', '2026-05-20 08:15:00', 4,  3),
('20D', 'B3', '2026-05-20 08:20:00', 5,  3),
-- Check-in vuelo 4 (SJO→MEX, abierto)
('5A',  'A1', '2026-05-20 12:00:00', 6,  4),
('5B',  'A1', '2026-05-20 12:05:00', 7,  4),
-- Check-in vuelo 5 (SJO→MIA, cerrado)
('1A',  'C2', '2026-05-19 05:00:00', 9,  5),
('1B',  'C2', '2026-05-19 05:05:00', 10, 5);



-- TABLA: MALETA
-- Cubre los 3 casos de cobro del enunciado:
--   1 maleta  = $0 adicional
--   2 maletas = $50 adicional
--   3 maletas = $125 adicional (0+50+75)
--   5 maletas = $275 adicional (0+50+75+75+75)
-- Cod_Reservacion de referencia (mismos que pase abordaje):
--   1=Carlos/v3  2=Ana/v3  3=Luis/v3  4=María/v3  5=Daniela/v3
--   9=Carlos/v5  10=Luis/v5

INSERT INTO MALETA (Peso, Color, Cod_Reservacion, ID_Usuario) VALUES
-- Carlos en vuelo 3: 1 maleta (gratis)
(23.5, 'Negro',   1, 1),

-- Ana en vuelo 3: 2 maletas ($50 adicional)
(18.0, 'Rojo',    2, 2),
(10.5, 'Azul',    2, 2),

-- Luis en vuelo 3: 3 maletas ($125 adicional: 0+50+75)
(22.0, 'Gris',    3, 5),
(15.0, 'Verde',   3, 5),
(8.0,  'Café',    3, 5),

-- María en vuelo 3: 1 maleta (gratis)
(20.0, 'Rosa',    4, 6),

-- Daniela en vuelo 3: 5 maletas ($275 adicional: 0+50+75+75+75)
(25.0, 'Negro',   5, 8),
(20.0, 'Blanco',  5, 8),
(15.0, 'Azul',    5, 8),
(12.0, 'Rojo',    5, 8),
(8.5,  'Gris',    5, 8),

-- Carlos en vuelo 5 (cerrado): 2 maletas ($50 adicional)
(24.0, 'Negro',   9, 1),
(14.0, 'Azul',    9, 1),

-- Luis en vuelo 5 (cerrado): 1 maleta (gratis)
(19.5, 'Verde',   10, 5);



-- TABLA: PROMOCION
-- Cubre promociones activas, inactivas y vencidas

INSERT INTO PROMOCION (Precio, Fecha_Inicio, Fecha_Fin, Imagen, Estado_Activa, ID_Aeropuerto_Origen, ID_Aeropuerto_Destino) VALUES
-- Activas
(199.99, '2026-05-01', '2026-06-30', 'img/promo_sjo_bog.jpg',  TRUE,  1, 3),  -- SJO → BOG
(149.99, '2026-05-15', '2026-07-15', 'img/promo_sjo_pty.jpg',  TRUE,  1, 5),  -- SJO → PTY
(299.99, '2026-06-01', '2026-08-31', 'img/promo_sjo_mia.jpg',  TRUE,  1, 8),  -- SJO → MIA
(249.99, '2026-05-20', '2026-07-20', 'img/promo_sjo_mex.jpg',  TRUE,  1, 6),  -- SJO → MEX
(179.99, '2026-06-01', '2026-09-01', 'img/promo_lir_mia.jpg',  TRUE,  2, 8),  -- LIR → MIA
-- Inactiva (desactivada manualmente)
(99.99,  '2026-04-01', '2026-06-30', 'img/promo_sjo_lim.jpg',  FALSE, 1, 4),  -- SJO → LIM
-- Vencida (fecha pasada, desactivada)
(129.99, '2026-01-01', '2026-03-31', NULL,                      FALSE, 1, 3);  -- SJO → BOG (vencida)



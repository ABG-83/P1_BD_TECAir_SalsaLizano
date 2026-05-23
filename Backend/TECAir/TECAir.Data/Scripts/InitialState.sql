-- -----------------------------------------------------------
--  TECAir — Initial State (Database Population)
--  Description: Test data insertion script.
--               Run AFTER Empty State.
-----------------------------------------------------------------


-- TABLE: users
-- Covers: regular clients, student clients with college ID/affiliation,
--         staff users, and an administrator.

INSERT INTO users (full_name, email, phone_number, role, miles, college_id_number, college) VALUES
-- Students (have college ID and affiliation)
('Carlos Mendoza Vargas',    'carlos.mendoza@estudiantec.cr',  '88001122', 'CLIENT', 1500, 'C-2021-0001', 'Instituto Tecnológico de Costa Rica'),
('Ana Sofía Jiménez Rojas',  'ana.jimenez@estudiantec.cr',     '88002233', 'CLIENT', 800,  'C-2022-0045', 'Instituto Tecnológico de Costa Rica'),
('Diego Hernández Mora',     'diego.hernandez@ucr.ac.cr',      '88003344', 'CLIENT', 200,  'B95432',      'Universidad de Costa Rica'),
('Valeria Torres Campos',    'valeria.torres@una.ac.cr',       '88004455', 'CLIENT', 3200, 'UNA-20190034','Universidad Nacional de Costa Rica'),
-- Regular clients (no college info)
('Luis Fernando Quesada',    'luis.quesada@gmail.com',         '88005566', 'CLIENT', 500,  NULL, NULL),
('María Elena Castillo',     'maria.castillo@gmail.com',       '88006677', 'CLIENT', 950,  NULL, NULL),
('Roberto Alvarado Pérez',   'roberto.alvarado@hotmail.com',   '88007788', 'CLIENT', 100,  NULL, NULL),
('Daniela Núñez Solís',      'daniela.nunez@outlook.com',      '88008899', 'CLIENT', 2700, NULL, NULL),
('Andrés Calvo Blanco',      'andres.calvo@gmail.com',         '88009900', 'CLIENT', 0,    NULL, NULL),
-- Airport staff
('Patricia Solano Vega',     'patricia.solano@tecair.cr',      '22001100', 'CLIENT', 0,    NULL, NULL),
('Marco Rodríguez Fallas',   'marco.rodriguez@tecair.cr',      '22002200', 'CLIENT', 0,    NULL, NULL),
-- Administrator
('Admin TECAir',             'admin@tecair.cr',                 '22003300', 'ADMIN',  0,    NULL, NULL);



-- TABLE: airplanes

INSERT INTO airplanes (plate_number, passenger_capacity, seat_count) VALUES
('TEC-001', 180, 180),
('TEC-002', 150, 150),
('TEC-003', 200, 200),
('TEC-004', 120, 120);



-- TABLE: airports

INSERT INTO airports (name, location) VALUES
('Aeropuerto Internacional Juan Santamaría',  'Alajuela, Costa Rica'),
('Aeropuerto Internacional Daniel Oduber',    'Liberia, Guanacaste, Costa Rica'),
('Aeropuerto Internacional El Dorado',        'Bogotá, Colombia'),
('Aeropuerto Internacional Jorge Chávez',     'Lima, Perú'),
('Aeropuerto Internacional Tocumen',          'Ciudad de Panamá, Panamá'),
('Aeropuerto Internacional Benito Juárez',    'Ciudad de México, México'),
('Aeropuerto Internacional Comairas',         'Buenos Aires, Argentina'),
('Aeropuerto de Miami',                       'Miami, Florida, Estados Unidos');



-- TABLE: flights
-- Covers different statuses: Scheduled, Boarding, Landed
-- Airports:
--   1 = Juan Santamaría (SJO)
--   2 = Daniel Oduber   (LIR)
--   3 = El Dorado       (BOG)
--   4 = Jorge Chávez    (LIM)
--   5 = Tocumen         (PTY)
--   6 = Benito Juárez   (MEX)
--   7 = Comairas        (EZE)
--   8 = Miami           (MIA)

INSERT INTO flights (flight_number, departure_time, arrival_time, status, airplane_plate_number, origin_airport_id, destination_airport_id) VALUES
-- TA-001: SJO -> BOG | Scheduled
('TA-001', '2026-06-01 06:00:00', '2026-06-01 09:30:00', 'Scheduled', 'TEC-001', 1, 3),
-- TA-002: SJO -> LIM | Scheduled
('TA-002', '2026-06-01 08:00:00', '2026-06-01 13:00:00', 'Scheduled', 'TEC-002', 1, 4),
-- TA-003: SJO -> PTY | Boarding (open for check-in demo)
('TA-003', '2026-05-20 10:00:00', '2026-05-20 11:30:00', 'Boarding',  'TEC-003', 1, 5),
-- TA-004: SJO -> MEX | Boarding
('TA-004', '2026-05-20 14:00:00', '2026-05-20 17:45:00', 'Boarding',  'TEC-004', 1, 6),
-- TA-005: SJO -> MIA | Landed (closed flight demo)
('TA-005', '2026-05-19 07:00:00', '2026-05-19 10:00:00', 'Landed',    'TEC-001', 1, 8),
-- TA-006: BOG -> LIM | Scheduled (with stop in PTY)
('TA-006', '2026-06-02 09:00:00', '2026-06-02 15:00:00', 'Scheduled', 'TEC-002', 3, 4),
-- TA-007: LIR -> MIA | Scheduled
('TA-007', '2026-06-03 11:00:00', '2026-06-03 15:30:00', 'Scheduled', 'TEC-003', 2, 8),
-- TA-008: SJO -> EZE | Scheduled (with stops)
('TA-008', '2026-06-05 05:00:00', '2026-06-05 20:00:00', 'Scheduled', 'TEC-004', 1, 7);



-- TABLE: flight_routes
-- Covers routes with 1 and 2 intermediate stops

INSERT INTO flight_routes (flight_number, airport_id, stop_order) VALUES
-- TA-006 (BOG -> LIM): stop in PTY
('TA-006', 5, 1),
-- TA-008 (SJO -> EZE): stop 1 in PTY, stop 2 in BOG
('TA-008', 5, 1),
('TA-008', 3, 2);



-- TABLE: reservations
-- Covers different payment statuses and flights

INSERT INTO reservations (date, payment_status, user_id, flight_number) VALUES
-- Reservations on TA-003 (Boarding — for check-in demo)
('2026-05-10 09:00:00', 'paid',      1, 'TA-003'),  -- Carlos   -> TA-003
('2026-05-10 09:30:00', 'paid',      2, 'TA-003'),  -- Ana      -> TA-003
('2026-05-11 10:00:00', 'paid',      5, 'TA-003'),  -- Luis     -> TA-003
('2026-05-11 10:30:00', 'paid',      6, 'TA-003'),  -- María    -> TA-003
('2026-05-12 11:00:00', 'paid',      8, 'TA-003'),  -- Daniela  -> TA-003
-- Reservations on TA-004 (Boarding)
('2026-05-13 08:00:00', 'paid',      3, 'TA-004'),  -- Diego    -> TA-004
('2026-05-13 08:30:00', 'paid',      4, 'TA-004'),  -- Valeria  -> TA-004
('2026-05-14 09:00:00', 'pending',   7, 'TA-004'),  -- Roberto  -> TA-004 (pending payment)
-- Reservations on TA-005 (Landed)
('2026-05-01 07:00:00', 'paid',      1, 'TA-005'),  -- Carlos   -> TA-005
('2026-05-01 07:30:00', 'paid',      5, 'TA-005'),  -- Luis     -> TA-005
-- Reservations on future flights
('2026-05-15 10:00:00', 'paid',      9, 'TA-001'),  -- Andrés   -> TA-001
('2026-05-15 10:30:00', 'paid',      2, 'TA-002'),  -- Ana      -> TA-002
('2026-05-16 11:00:00', 'cancelled', 6, 'TA-001');  -- María    -> TA-001 (cancelled)



-- TABLE: check_ins
-- Only passengers who have checked in have a boarding pass.
-- Covers Boarding flights (TA-003, TA-004) and Landed flight (TA-005).
-- reservation_id reference:
--   1 = Carlos/TA-003   2 = Ana/TA-003   3 = Luis/TA-003
--   4 = María/TA-003    5 = Daniela/TA-003
--   6 = Diego/TA-004    7 = Valeria/TA-004

INSERT INTO check_ins (seat, boarding_gate, print_time, reservation_id, flight_number) VALUES
-- Check-in TA-003 (SJO->PTY, Boarding)
('12A', 'B3', '2026-05-20 08:00:00', 1,  'TA-003'),
('12B', 'B3', '2026-05-20 08:05:00', 2,  'TA-003'),
('14C', 'B3', '2026-05-20 08:10:00', 3,  'TA-003'),
('15A', 'B3', '2026-05-20 08:15:00', 4,  'TA-003'),
('20D', 'B3', '2026-05-20 08:20:00', 5,  'TA-003'),
-- Check-in TA-004 (SJO->MEX, Boarding)
('5A',  'A1', '2026-05-20 12:00:00', 6,  'TA-004'),
('5B',  'A1', '2026-05-20 12:05:00', 7,  'TA-004'),
-- Check-in TA-005 (SJO->MIA, Landed)
('1A',  'C2', '2026-05-19 05:00:00', 9,  'TA-005'),
('1B',  'C2', '2026-05-19 05:05:00', 10, 'TA-005');



-- TABLE: baggages
-- Covers the 3 billing tiers from the spec:
--   1 bag  = $0 extra
--   2 bags = $50 extra
--   3 bags = $125 extra (0+50+75)
--   5 bags = $275 extra (0+50+75+75+75)
-- reservation_id reference (same as check_ins):
--   1=Carlos/TA-003  2=Ana/TA-003  3=Luis/TA-003  4=María/TA-003  5=Daniela/TA-003
--   9=Carlos/TA-005  10=Luis/TA-005

INSERT INTO baggages (weight, color, reservation_id, user_id) VALUES
-- Carlos on TA-003: 1 bag (free)
(23.5, 'Negro',   1, 1),

-- Ana on TA-003: 2 bags ($50 extra)
(18.0, 'Rojo',    2, 2),
(10.5, 'Azul',    2, 2),

-- Luis on TA-003: 3 bags ($125 extra: 0+50+75)
(22.0, 'Gris',    3, 5),
(15.0, 'Verde',   3, 5),
(8.0,  'Café',    3, 5),

-- María on TA-003: 1 bag (free)
(20.0, 'Rosa',    4, 6),

-- Daniela on TA-003: 5 bags ($275 extra: 0+50+75+75+75)
(25.0, 'Negro',   5, 8),
(20.0, 'Blanco',  5, 8),
(15.0, 'Azul',    5, 8),
(12.0, 'Rojo',    5, 8),
(8.5,  'Gris',    5, 8),

-- Carlos on TA-005 (Landed): 2 bags ($50 extra)
(24.0, 'Negro',   9, 1),
(14.0, 'Azul',    9, 1),

-- Luis on TA-005 (Landed): 1 bag (free)
(19.5, 'Verde',   10, 5);



-- TABLE: promotions
-- Covers active, inactive, and expired promotions

INSERT INTO promotions (price, start_date, end_date, image, is_active, origin_airport_id, destination_airport_id) VALUES
-- Active
(199.99, '2026-05-01', '2026-06-30', 'img/promo_sjo_bog.jpg',  TRUE,  1, 3),  -- SJO -> BOG
(149.99, '2026-05-15', '2026-07-15', 'img/promo_sjo_pty.jpg',  TRUE,  1, 5),  -- SJO -> PTY
(299.99, '2026-06-01', '2026-08-31', 'img/promo_sjo_mia.jpg',  TRUE,  1, 8),  -- SJO -> MIA
(249.99, '2026-05-20', '2026-07-20', 'img/promo_sjo_mex.jpg',  TRUE,  1, 6),  -- SJO -> MEX
(179.99, '2026-06-01', '2026-09-01', 'img/promo_lir_mia.jpg',  TRUE,  2, 8),  -- LIR -> MIA
-- Inactive (manually deactivated)
(99.99,  '2026-04-01', '2026-06-30', 'img/promo_sjo_lim.jpg',  FALSE, 1, 4),  -- SJO -> LIM
-- Expired (past date, deactivated)
(129.99, '2026-01-01', '2026-03-31', NULL,                      FALSE, 1, 3);  -- SJO -> BOG (expired)

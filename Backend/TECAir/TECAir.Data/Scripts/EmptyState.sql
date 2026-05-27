
-- TECAir — Empty State (Database Creation)
-- Structure-only script with no data.
-- NOTE: This database must be created with UTF-8 encoding. If you see garbled
-- characters, drop and recreate it first with:
--   CREATE DATABASE tecair_dev WITH ENCODING 'UTF8' TEMPLATE template0;
SET client_encoding TO 'UTF8';

-- Drop tables if they exist (ordered by FK dependencies)
DROP TABLE IF EXISTS payments       CASCADE;
DROP TABLE IF EXISTS flight_routes   CASCADE;
DROP TABLE IF EXISTS check_ins       CASCADE;
DROP TABLE IF EXISTS baggages        CASCADE;
DROP TABLE IF EXISTS reservations    CASCADE;
DROP TABLE IF EXISTS promotions      CASCADE;
DROP TABLE IF EXISTS flights         CASCADE;
DROP TABLE IF EXISTS airports        CASCADE;
DROP TABLE IF EXISTS airplanes       CASCADE;
DROP TABLE IF EXISTS users           CASCADE;

-- TABLE: users
CREATE TABLE users (
    user_id            SERIAL          PRIMARY KEY,
    full_name          VARCHAR(150)    NOT NULL,
    email              VARCHAR(150)    NOT NULL UNIQUE,
    phone_number       VARCHAR(20)     NOT NULL,
    role               VARCHAR(30)     NOT NULL
                                       CHECK (role IN ('ADMIN', 'CLIENT')),
    miles              REAL            NOT NULL DEFAULT 0
                                       CHECK (miles >= 0),
    college_id_number  VARCHAR(50),
    college            VARCHAR(150),
    password_hash      VARCHAR(255)    NOT NULL DEFAULT ''
);

COMMENT ON TABLE  users                     IS 'System users: clients and airport staff.';
COMMENT ON COLUMN users.college_id_number   IS 'University student ID. Applies only if the user is a student.';
COMMENT ON COLUMN users.college             IS 'University affiliation. Applies only if the user is a student.';

-- TABLE: airplanes
CREATE TABLE airplanes (
    plate_number       VARCHAR(20)     PRIMARY KEY,
    passenger_capacity INT             NOT NULL CHECK (passenger_capacity > 0),
    seat_count         INT             NOT NULL CHECK (seat_count > 0)
);

COMMENT ON TABLE airplanes IS 'Airplanes available in the TECAir fleet.';

-- TABLE: airports
CREATE TABLE airports (
    airport_id SERIAL          PRIMARY KEY,
    name       VARCHAR(150)    NOT NULL,
    location   VARCHAR(200)    NOT NULL
);

COMMENT ON TABLE airports IS 'Origin, destination, and layover airports for flights.';

-- TABLE: flights
CREATE TABLE flights (
    flight_number          VARCHAR(20)     PRIMARY KEY,
    departure_time         TIMESTAMP       NOT NULL,
    arrival_time           TIMESTAMP       NOT NULL,
    status                 VARCHAR(20)     NOT NULL DEFAULT 'Scheduled'
                                           CHECK (status IN ('Scheduled', 'Boarding', 'Delayed', 'InAir', 'Landed', 'Cancelled')),
    price                  DECIMAL(10,2)   NOT NULL DEFAULT 0
                                           CHECK (price >= 0),
    airplane_plate_number  VARCHAR(20)     NOT NULL,
    origin_airport_id      INT             NOT NULL,
    destination_airport_id INT             NOT NULL,

    CONSTRAINT fk_flight_airplane
        FOREIGN KEY (airplane_plate_number)
        REFERENCES airplanes(plate_number)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_flight_origin
        FOREIGN KEY (origin_airport_id)
        REFERENCES airports(airport_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_flight_destination
        FOREIGN KEY (destination_airport_id)
        REFERENCES airports(airport_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT chk_flight_times
        CHECK (arrival_time > departure_time),

    CONSTRAINT chk_flight_airports
        CHECK (origin_airport_id <> destination_airport_id)
);

COMMENT ON TABLE  flights                IS 'Flights registered in TECAir with route and assigned airplane.';
COMMENT ON COLUMN flights.status         IS 'Scheduled -> Boarding -> InAir -> Landed. May also be Delayed or Cancelled.';
COMMENT ON COLUMN flights.departure_time IS 'Exact departure date and time of the flight.';
COMMENT ON COLUMN flights.arrival_time   IS 'Estimated arrival date and time. Must be after departure_time.';

-- TABLE: flight_routes
CREATE TABLE flight_routes (
    flight_number VARCHAR(20) NOT NULL,
    airport_id    INT         NOT NULL,
    stop_order    INT         NOT NULL CHECK (stop_order >= 1),

    CONSTRAINT pk_flight_routes
        PRIMARY KEY (flight_number, airport_id),

    CONSTRAINT fk_route_flight
        FOREIGN KEY (flight_number)
        REFERENCES flights(flight_number)
        ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT fk_route_airport
        FOREIGN KEY (airport_id)
        REFERENCES airports(airport_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE  flight_routes            IS 'Layover airports for each flight, with their order in the route.';
COMMENT ON COLUMN flight_routes.stop_order IS 'Position of the stop in the route: 1 = first stop, 2 = second, etc.';

-- TABLE: reservations
CREATE TABLE reservations (
    reservation_code VARCHAR(30) PRIMARY KEY,
    date             TIMESTAMP   NOT NULL DEFAULT NOW(),
    payment_state    VARCHAR(20) NOT NULL DEFAULT 'Pending'
                                  CHECK (payment_state IN ('Pending', 'Paid', 'Failed', 'Refunded')),
    user_id          INT         NOT NULL,
    flight_number    VARCHAR(20) NOT NULL,
    seat_count       INT         NOT NULL DEFAULT 1 CHECK (seat_count > 0),

    CONSTRAINT fk_reservation_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_reservation_flight
        FOREIGN KEY (flight_number)
        REFERENCES flights(flight_number)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE reservations IS 'Passenger reservations for specific flights.';

-- TABLE: check_ins
CREATE TABLE check_ins (
    checkin_id      SERIAL      PRIMARY KEY,
    seat            VARCHAR(10) NOT NULL,
    boarding_gate   VARCHAR(10) NOT NULL,
    print_time      TIMESTAMP   NOT NULL DEFAULT NOW(),
    reservation_code VARCHAR(30) NOT NULL,
    flight_number   VARCHAR(20) NOT NULL,

    CONSTRAINT fk_checkin_reservation
        FOREIGN KEY (reservation_code)
        REFERENCES reservations(reservation_code)
        ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT fk_checkin_flight
        FOREIGN KEY (flight_number)
        REFERENCES flights(flight_number)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE  check_ins                  IS 'Boarding passes generated when checking in for a reservation.';
COMMENT ON COLUMN check_ins.reservation_code IS 'Links the check-in to its respective reservation.';

-- TABLE: baggages
CREATE TABLE baggages (
    baggage_id      SERIAL        PRIMARY KEY,
    weight          DECIMAL(6,2)  NOT NULL CHECK (weight > 0),
    color           VARCHAR(50)   NOT NULL,
    reservation_code VARCHAR(30)   NOT NULL,
    user_id         INT           NOT NULL,

    CONSTRAINT fk_baggage_reservation
        FOREIGN KEY (reservation_code)
        REFERENCES reservations(reservation_code)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_baggage_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE baggages IS 'Bags assigned to checked-in passengers. First bag is free, 2nd costs $50, additional ones cost $75 each.';

-- TABLE: payments
CREATE TABLE payments (
    payment_id           SERIAL         PRIMARY KEY,
    reservation_code     VARCHAR(30)    NOT NULL,
    amount               DECIMAL(10,2)  NOT NULL CHECK (amount >= 0),
    transaction_date     TIMESTAMP      NOT NULL DEFAULT NOW(),
    transaction_reference VARCHAR(100)   NOT NULL,
    payment_status       VARCHAR(20)    NOT NULL DEFAULT 'Pending'
                                        CHECK (payment_status IN ('Pending', 'Paid', 'Failed', 'Refunded')),

    CONSTRAINT fk_payment_reservation
        FOREIGN KEY (reservation_code)
        REFERENCES reservations(reservation_code)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

COMMENT ON TABLE payments IS 'Ledger entries for payment attempts and settlements linked to reservations.';

-- TABLE: promotions
CREATE TABLE promotions (
    promotion_id            SERIAL          PRIMARY KEY,
    price                   DECIMAL(10,2)   NOT NULL CHECK (price > 0),
    start_date              DATE            NOT NULL,
    end_date                DATE            NOT NULL,
    image                   VARCHAR(300),
    is_active               BOOLEAN         NOT NULL DEFAULT TRUE,
    origin_airport_id       INT             NOT NULL,
    destination_airport_id  INT             NOT NULL,

    CONSTRAINT fk_promo_origin
        FOREIGN KEY (origin_airport_id)
        REFERENCES airports(airport_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT fk_promo_destination
        FOREIGN KEY (destination_airport_id)
        REFERENCES airports(airport_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,

    CONSTRAINT chk_promo_dates
        CHECK (end_date > start_date),

    CONSTRAINT chk_promo_airports
        CHECK (origin_airport_id <> destination_airport_id)
);

COMMENT ON TABLE  promotions           IS 'Price promotions between airport pairs with defined validity periods.';
COMMENT ON COLUMN promotions.image     IS 'Path or URL of the promotion image. Optional.';
COMMENT ON COLUMN promotions.is_active IS 'TRUE = active and visible promotion. FALSE = deactivated.';

-- Additional indexes for performance on frequent queries
CREATE INDEX idx_flight_departure      ON flights(departure_time);
CREATE INDEX idx_flight_origin         ON flights(origin_airport_id);
CREATE INDEX idx_flight_destination    ON flights(destination_airport_id);
CREATE INDEX idx_reservation_user      ON reservations(user_id);
CREATE INDEX idx_reservation_flight    ON reservations(flight_number);
CREATE INDEX idx_checkin_reservation   ON check_ins(reservation_code);
CREATE INDEX idx_baggage_reservation   ON baggages(reservation_code);
CREATE INDEX idx_payment_reservation    ON payments(reservation_code);
CREATE INDEX idx_promo_active          ON promotions(is_active);

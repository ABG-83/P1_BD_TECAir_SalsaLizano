-- =============================================================================
-- Migration: Add price column to flights table
-- Run this script on existing databases created before this change.
-- =============================================================================
ALTER TABLE flights
    ADD COLUMN IF NOT EXISTS price DECIMAL(10,2) NOT NULL DEFAULT 0
        CHECK (price >= 0);

-- Set representative prices for existing seed flights
UPDATE flights SET price = 350.00 WHERE flight_number = 'TA-001';
UPDATE flights SET price = 450.00 WHERE flight_number = 'TA-002';
UPDATE flights SET price = 180.00 WHERE flight_number = 'TA-003';
UPDATE flights SET price = 320.00 WHERE flight_number = 'TA-004';
UPDATE flights SET price = 280.00 WHERE flight_number = 'TA-005';
UPDATE flights SET price = 220.00 WHERE flight_number = 'TA-006';
UPDATE flights SET price = 310.00 WHERE flight_number = 'TA-007';
UPDATE flights SET price = 520.00 WHERE flight_number = 'TA-008';

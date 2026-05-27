-- =============================================================================
-- Migration: Add password_hash column to users table
-- Run this script on existing databases that were created before this change.
-- =============================================================================
SET client_encoding TO 'UTF8';

-- pgcrypto is required to generate BCrypt-compatible hashes
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Add column if it does not already exist (idempotent)
ALTER TABLE users
    ADD COLUMN IF NOT EXISTS password_hash VARCHAR(255) NOT NULL DEFAULT '';

-- Set a default password ("password123") for all existing users who have no hash.
-- Users should be encouraged to change their password after first login.
UPDATE users
SET password_hash = crypt('password123', gen_salt('bf'))
WHERE password_hash = '';

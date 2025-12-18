-- Migration: Add LastAccessedDate to Surveys table
-- Date: 2025-12-17
-- Description: Add LastAccessedDate column to track when surveys are accessed for analytics

-- SQLite version
ALTER TABLE Surveys ADD COLUMN LastAccessedDate TEXT;

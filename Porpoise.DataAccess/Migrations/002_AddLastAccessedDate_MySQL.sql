-- Migration: Add LastAccessedDate to Surveys table
-- Date: 2025-12-17
-- Description: Add LastAccessedDate column to track when surveys are accessed for analytics

-- MySQL version - conditional column addition
SET @dbname = DATABASE();
SET @tablename = 'Surveys';
SET @columnname = 'LastAccessedDate';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE (table_name = @tablename) AND (table_schema = @dbname) AND (column_name = @columnname)) > 0,
  'SELECT 1',
  CONCAT('ALTER TABLE ', @tablename, ' ADD COLUMN ', @columnname, ' DATETIME NULL')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

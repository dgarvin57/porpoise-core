# Database Setup

This project uses MySQL for data persistence. This document explains how to set up and use the database.

## Prerequisites

- MySQL 8.0 or higher installed locally
- .NET 9.0 or higher SDK

## Initial Setup

### 1. Create Database

Run the database creation script:

```bash
mysql -u root -p < Porpoise.DataAccess/Scripts/01_CreateDatabase.sql
```

This creates the `porpoise_dev` database with UTF-8 encoding.

### 2. Create Tables

Run the table creation script:

```bash
mysql -u root -p porpoise_dev < Porpoise.DataAccess/Scripts/02_RecreateTables.sql
```

This creates the following tables:
- **Surveys** - Core survey metadata
- **Questions** - Survey questions with foreign key to Surveys
- **Responses** - Question responses with foreign key to Questions
- **SurveyResponses** - Response tracking for surveys
- **SurveyData** - Raw survey data storage

### 3. Configure Connection String

Copy `appsettings.json.template` to `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "PorpoiseDb": "Server=localhost;Port=3306;Database=porpoise_dev;User=root;Password=YOUR_PASSWORD;CharSet=utf8mb4;"
  }
}
```

**Note:** `appsettings.json` is gitignored to keep credentials secure.

## Database Schema

### Surveys Table
```sql
CREATE TABLE Surveys (
    Id CHAR(36) PRIMARY KEY,
    SurveyName VARCHAR(255) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    LockStatus INT NOT NULL DEFAULT 0,
    UnlockKeyName VARCHAR(255),
    UnlockKeyType INT NOT NULL DEFAULT 0,
    SaveAlteredString VARCHAR(500),
    SurveyFileName VARCHAR(255),
    DataFileName VARCHAR(255),
    OrigDataFilePath VARCHAR(500),
    SurveyPath VARCHAR(500),
    SurveyFolder VARCHAR(500),
    FullProjectFolder VARCHAR(500),
    ErrorsExist BOOLEAN NOT NULL DEFAULT TRUE,
    SurveyNotes TEXT,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_survey_name (SurveyName),
    INDEX idx_status (Status)
);
```

### Questions Table
```sql
CREATE TABLE Questions (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    QstNumber INT NOT NULL,
    QstLabel VARCHAR(255),
    DataFileColumn INT,
    VariableType VARCHAR(50),
    MissingLow DOUBLE,
    MissingHigh DOUBLE,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE
);
```

### Responses Table
```sql
CREATE TABLE Responses (
    Id CHAR(36) PRIMARY KEY,
    QuestionId CHAR(36) NOT NULL,
    RespValue DOUBLE NOT NULL,
    Label VARCHAR(500),
    Percentage DOUBLE,
    Frequency INT,
    IndexType VARCHAR(50),
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);
```

## Running Tests

### Unit Tests (with Mocks)
```bash
dotnet test --filter "FullyQualifiedName!~Integration"
```

### Integration Tests (with MySQL)
```bash
# Make sure MySQL is running and database is created
dotnet test --filter "FullyQualifiedName~Integration"
```

### All Tests
```bash
dotnet test
```

## Environment Variables

You can override the connection string for integration tests:

```bash
export PORPOISE_TEST_CONNECTION="Server=localhost;Port=3306;Database=porpoise_test;User=testuser;Password=testpass;CharSet=utf8mb4;"
dotnet test --filter "FullyQualifiedName~Integration"
```

## Database Migrations

This project uses a simple SQL script approach for schema management. To update the schema:

1. Create a new SQL script in `Porpoise.DataAccess/Scripts/`
2. Number it sequentially (e.g., `03_AddNewColumns.sql`)
3. Document changes in this README
4. Run the script: `mysql -u root -p porpoise_dev < Porpoise.DataAccess/Scripts/03_AddNewColumns.sql`

## Troubleshooting

### Connection Errors
- Verify MySQL is running: `brew services list | grep mysql`
- Test connection: `mysql -u root -p`
- Check firewall settings if using remote MySQL

### Schema Issues
- Drop and recreate: Run `02_RecreateTables.sql` (includes DROP statements)
- Check table structure: `mysql -u root -p -e "USE porpoise_dev; DESCRIBE Surveys;"`

### Test Failures
- Ensure database is created and tables exist
- Verify connection string in appsettings.json or PORPOISE_TEST_CONNECTION
- Check MySQL logs for detailed error messages

## Production Considerations

For production deployment:

1. Use a dedicated database user (not root)
2. Store connection strings in environment variables or secure vault
3. Use SSL/TLS for database connections
4. Implement proper backup strategy
5. Consider connection pooling settings in MySqlConnector
6. Monitor query performance and add indexes as needed

## Tools

### Recommended MySQL Clients
- **Tables IDE** - Used by this project's author
- **MySQL Workbench** - Official MySQL GUI
- **DataGrip** - JetBrains database tool
- **DBeaver** - Free universal database tool

### Command Line
```bash
# Connect to database
mysql -u root -p porpoise_dev

# Show all tables
SHOW TABLES;

# Show table structure
DESCRIBE Surveys;

# Count records
SELECT COUNT(*) FROM Surveys;

# View recent surveys
SELECT Id, SurveyName, Status, CreatedDate FROM Surveys ORDER BY CreatedDate DESC LIMIT 10;
```

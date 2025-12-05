using Dapper;
using Porpoise.DataAccess.Context;
using System;
using System.Threading.Tasks;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Shared fixture for integration tests to ensure test tenant exists before any tests run.
/// This fixture creates the tenant once and is shared across all integration tests.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public DapperContext Context { get; }
    public string TestTenantId { get; }
    public string TestTenantKey { get; }

    public DatabaseFixture()
    {
        Context = new DapperContext("Server=localhost;Port=3306;Database=porpoise_dev;User=root;Password=Dg5901%1;CharSet=utf8mb4;");
        
        // Use a predictable GUID for test tenant
        TestTenantId = "00000000-0000-0000-0000-000000000001";
        TestTenantKey = "integration-test-tenant";
    }

    public async Task InitializeAsync()
    {
        // Create test tenant
        await CreateTestTenantAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up test tenant (this will cascade delete all test data)
        await DeleteTestTenantAsync();
    }

    private async Task CreateTestTenantAsync()
    {
        const string sql = @"
            INSERT INTO Tenants (TenantId, TenantKey, Name, IsActive, CreatedBy, CreatedDate, ModifiedDate)
            VALUES (@TenantId, @TenantKey, @Name, @IsActive, @CreatedBy, @CreatedDate, @ModifiedDate)
            ON DUPLICATE KEY UPDATE 
                Name = VALUES(Name),
                ModifiedDate = VALUES(ModifiedDate);";

        using var connection = Context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            TenantId = TestTenantId,
            TenantKey = TestTenantKey,
            Name = "Integration Test Tenant",
            IsActive = true,
            CreatedBy = "IntegrationTests",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        });
    }

    private async Task DeleteTestTenantAsync()
    {
        const string sql = "DELETE FROM Tenants WHERE TenantId = @TenantId";
        
        using var connection = Context.CreateConnection();
        await connection.ExecuteAsync(sql, new { TenantId = TestTenantId });
    }
}

/// <summary>
/// Collection definition to share the database fixture across all integration tests
/// and ensure they run sequentially (not in parallel) to avoid conflicts.
/// </summary>
[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

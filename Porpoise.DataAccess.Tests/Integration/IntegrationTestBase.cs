using Porpoise.DataAccess.Context;
using System;
using System.Threading.Tasks;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Base class for integration tests that need database access.
/// Uses a shared DatabaseFixture to ensure the test tenant exists.
/// All tests using this base class should be in the "Database" collection.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly DapperContext Context;
    protected readonly string TestTenantId;
    protected readonly string TestTenantKey;

    protected IntegrationTestBase(DatabaseFixture fixture)
    {
        Context = fixture.Context;
        TestTenantId = fixture.TestTenantId;
        TestTenantKey = fixture.TestTenantKey;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;
}

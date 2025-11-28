#nullable enable

using System.Data;
using MySqlConnector;

namespace Porpoise.DataAccess.Context;

/// <summary>
/// Database context for creating MySQL connections.
/// This is the simple Dapper approach—no migrations, just connection management.
/// </summary>
public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Create a new MySQL connection.
    /// Use this in repositories to execute queries.
    /// </summary>
    public IDbConnection CreateConnection()
        => new MySqlConnection(_connectionString);
}

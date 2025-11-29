#nullable enable

using System;
using System.Data;
using Dapper;
using MySqlConnector;

namespace Porpoise.DataAccess.Context;

/// <summary>
/// Database context for creating MySQL connections.
/// This is the simple Dapper approach—no migrations, just connection management.
/// </summary>
public class DapperContext
{
    private readonly string _connectionString;
    private static bool _typeHandlersConfigured = false;

    public DapperContext(string connectionString)
    {
        _connectionString = connectionString;
        
        // Configure type handlers once
        if (!_typeHandlersConfigured)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            _typeHandlersConfigured = true;
        }
    }

    /// <summary>
    /// Create a new MySQL connection.
    /// Use this in repositories to execute queries.
    /// </summary>
    public IDbConnection CreateConnection()
        => new MySqlConnection(_connectionString);
}

/// <summary>
/// Type handler for converting VARCHAR(36) GUID columns to System.Guid
/// </summary>
public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        if (value is string stringValue)
        {
            return Guid.Parse(stringValue);
        }
        if (value is Guid guidValue)
        {
            return guidValue;
        }
        throw new InvalidCastException($"Cannot convert {value?.GetType().Name ?? "null"} to Guid");
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}

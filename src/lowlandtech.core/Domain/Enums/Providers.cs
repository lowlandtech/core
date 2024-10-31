namespace LowlandTech.Core.Domain.Enums;

/// <summary>
/// Domain database providers.
/// </summary>
public enum Providers
{
    /// <summary>
    /// Gets the InMemory provider.
    /// </summary>
    InMemory,

    /// <summary>
    /// Gets the SQLite provider.
    /// </summary>
    Sqlite,

    /// <summary>
    /// Gets the PostgreSQL provider.
    /// </summary>
    PgSql,

    /// <summary>
    /// Gets the SQL Server provider.
    /// </summary>
    SqlServer,

    /// <summary>
    /// Gets the MySQL provider.
    /// </summary>
    MySql,
}
namespace PetLoveCommunity.API.Services;

/// <summary>
/// Service interface for generating database connection strings.
/// </summary>
public interface IDatabaseConnectionService
{
    /// <summary>
    /// Gets the connection string for the default database.
    /// </summary>
    /// <returns>A formatted PostgreSQL connection string.</returns>
    string GetConnectionString();
    
    /// <summary>
    /// Gets the connection string for the specified database.
    /// </summary>
    /// <param name="databaseName">The name of the database to connect to.</param>
    /// <returns>A formatted PostgreSQL connection string.</returns>
    string GetConnectionString(string databaseName);
}
namespace PetLoveCommunity.API.Configuration;

/// <summary>
/// Configuration interface for database connection credentials.
/// </summary>
public interface IDatabaseCredentials
{
    /// <summary>
    /// The username for database connection.
    /// </summary>
    string Username { get; }
    
    /// <summary>
    /// The password for database connection.
    /// </summary>
    string Password { get; }
    
    /// <summary>
    /// The name of the database to connect to.
    /// </summary>
    string DatabaseName { get; }
}
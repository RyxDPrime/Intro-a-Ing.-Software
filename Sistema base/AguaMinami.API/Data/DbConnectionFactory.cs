using Microsoft.Data.SqlClient;
using System.Data;

namespace AguaMinami.API.Data;

/// <summary>
/// Fábrica de conexiones — desacopla los repositorios del proveedor de datos concreto.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AguaMinami")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'AguaMinami'.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}

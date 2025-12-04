using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace UserManagement.Oracle;

internal sealed class OracleConnectionFactory(string? connectionString = null) : IOracleConnectionFactory
{
    private readonly string _connectionString = connectionString
                            ?? ConfigurationManager
                                   .ConnectionStrings["OracleDb"]
                                   .ConnectionString;

    public OracleConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }
}

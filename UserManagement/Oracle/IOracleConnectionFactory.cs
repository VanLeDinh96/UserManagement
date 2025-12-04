using Oracle.ManagedDataAccess.Client;

namespace UserManagement.Oracle;

public interface IOracleConnectionFactory
{
    /// <summary>
    /// Create a new OracleConnection instance (not opened yet).
    /// </summary>
    OracleConnection CreateConnection();
}

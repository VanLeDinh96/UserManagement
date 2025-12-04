using Oracle.ManagedDataAccess.Client;

namespace UserManagement.Oracle;

public interface IOracleStoredProcExecutor
{
    /// <summary>
    /// Execute a stored procedure that does not return a value.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string storedProcName, params OracleParameter[] parameters);

    /// <summary>
    /// Execute a stored function (CommandType.StoredProcedure with ReturnValue parameter)
    /// and convert the return value to type T.
    /// </summary>
    Task<T> ExecuteFunctionAsync<T>(
        string storedFunctionName,
        OracleParameter returnParameter,
        params OracleParameter[] parameters);
}

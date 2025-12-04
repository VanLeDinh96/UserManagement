using Oracle.ManagedDataAccess.Client;
using System.Data;
using UserManagement.Extensions;

namespace UserManagement.Oracle;

internal sealed class OracleStoredProcExecutor(IOracleConnectionFactory connectionFactory) : IOracleStoredProcExecutor
{
    private readonly IOracleConnectionFactory _connectionFactory = connectionFactory
                             ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<int> ExecuteNonQueryAsync(string storedProcName, params OracleParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(storedProcName))
            throw new ArgumentException("Stored procedure name must not be empty.", nameof(storedProcName));

        await using var connection = _connectionFactory.CreateConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = storedProcName;
        command.CommandType = CommandType.StoredProcedure;
        command.BindByName = true;

        if (parameters is { Length: > 0 })
            command.Parameters.AddRange(parameters);

        await connection.OpenAsync().ConfigureAwait(false);
        return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<T> ExecuteFunctionAsync<T>(
        string storedFunctionName,
        OracleParameter returnParameter,
        params OracleParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(storedFunctionName))
            throw new ArgumentException("Stored function name must not be empty.", nameof(storedFunctionName));
        if (returnParameter == null)
            throw new ArgumentNullException(nameof(returnParameter));

        await using var connection = _connectionFactory.CreateConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = storedFunctionName;
        command.CommandType = CommandType.StoredProcedure;
        command.BindByName = true;

        // Add return value parameter first
        command.Parameters.Add(returnParameter);

        if (parameters is { Length: > 0 })
            command.Parameters.AddRange(parameters);

        await connection.OpenAsync().ConfigureAwait(false);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);

        // Normalize Oracle-specific value to T
        return OracleValueConverter.ConvertTo<T>(returnParameter.Value);
    }
}

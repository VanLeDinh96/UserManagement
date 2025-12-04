using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace UserManagement.Extensions;

public static class OracleParameterFactory
{
    /// <summary>
    /// Create an input parameter.
    /// </summary>
    public static OracleParameter In(
        string name,
        OracleDbType dbType,
        object? value)
    {
        return new OracleParameter
        {
            ParameterName = name,
            OracleDbType = dbType,
            Direction = ParameterDirection.Input,
            Value = value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Create an output parameter.
    /// </summary>
    public static OracleParameter Out(
        string name,
        OracleDbType dbType,
        int size = 0)
    {
        var p = new OracleParameter
        {
            ParameterName = name,
            OracleDbType = dbType,
            Direction = ParameterDirection.Output
        };

        if (size > 0)
            p.Size = size;

        return p;
    }

    /// <summary>
    /// Create a return value parameter for stored function.
    /// </summary>
    public static OracleParameter Return(
        OracleDbType dbType,
        string name = "p_return")
    {
        return new OracleParameter
        {
            ParameterName = name,
            OracleDbType = dbType,
            Direction = ParameterDirection.ReturnValue
        };
    }
}

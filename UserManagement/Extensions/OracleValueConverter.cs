using Oracle.ManagedDataAccess.Types;
using System.Globalization;

namespace UserManagement.Extensions;

public static class OracleValueConverter
{
    /// <summary>
    /// Convert an Oracle provider-specific value to a desired CLR type.
    /// Handles OracleDecimal, OracleString, OracleDate, etc.
    /// </summary>
    public static T ConvertTo<T>(object? value)
    {
        if (value == null || value is DBNull)
            return default!;

        var targetType = typeof(T);

        // If it already is the requested type, just cast.
        if (targetType.IsInstanceOfType(value))
            return (T)value;

        if (targetType == typeof(int))
            return (T)(object)ToInt32(value);

        if (targetType == typeof(long))
            return (T)(object)ToInt64(value);

        if (targetType == typeof(decimal))
            return (T)(object)ToDecimal(value);

        if (targetType == typeof(string))
            return (T)(object)ToStringValue(value);

        if (targetType == typeof(DateTime))
            return (T)(object)ToDateTime(value);

        if (targetType == typeof(bool))
            return (T)(object)ToBool(value);

        // Fallback: try .NET conversion
        return (T)Convert.ChangeType(
            value,
            targetType,
            CultureInfo.InvariantCulture);
    }

    private static int ToInt32(object value)
    {
        return value switch
        {
            int i => i,
            long l => Convert.ToInt32(l),
            decimal d => Convert.ToInt32(d),
            OracleDecimal od when !od.IsNull => od.ToInt32(), // fix OracleDecimal issue
            OracleDecimal odNull when odNull.IsNull => 0,
            _ => Convert.ToInt32(value, CultureInfo.InvariantCulture)
        };
    }

    private static long ToInt64(object value)
    {
        return value switch
        {
            long l => l,
            int i => i,
            decimal d => Convert.ToInt64(d),
            OracleDecimal od when !od.IsNull => od.ToInt64(),
            OracleDecimal odNull when odNull.IsNull => 0L,
            _ => Convert.ToInt64(value, CultureInfo.InvariantCulture)
        };
    }

    private static decimal ToDecimal(object value)
    {
        return value switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            OracleDecimal od when !od.IsNull => od.Value,
            OracleDecimal odNull when odNull.IsNull => 0m,
            _ => Convert.ToDecimal(value, CultureInfo.InvariantCulture)
        };
    }

    private static string ToStringValue(object value)
    {
        return value switch
        {
            string s => s,
            OracleString os when !os.IsNull => os.Value,
            OracleString osNull when osNull.IsNull => string.Empty,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static DateTime ToDateTime(object value)
    {
        return value switch
        {
            DateTime dt => dt,
            OracleDate od when !od.IsNull => od.Value,
            OracleDate odNull when odNull.IsNull => DateTime.MinValue,
            _ => Convert.ToDateTime(value, CultureInfo.InvariantCulture)
        };
    }

    private static bool ToBool(object value)
    {
        // Common patterns: NUMBER(1) 0/1, CHAR('Y'/'N'), etc.
        return value switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0L,
            decimal d => d != 0m,
            OracleDecimal od when !od.IsNull => od.ToInt32() != 0,
            string s => s == "Y" || s == "y" || s == "1" || bool.Parse(s),
            OracleString os when !os.IsNull =>
                os.Value == "Y" || os.Value == "y" || os.Value == "1",
            _ => Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0
        };
    }
}

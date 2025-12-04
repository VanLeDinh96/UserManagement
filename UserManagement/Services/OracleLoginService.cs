using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using UserManagement.Extensions;
using UserManagement.Models;

namespace UserManagement.Services;

public sealed class OracleLoginService(string? connectionString = null) : ILoginService
{
    private readonly string _connectionString = connectionString
                            ?? ConfigurationManager
                                   .ConnectionStrings["OracleDb"]
                                   .ConnectionString;

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Username must not be empty."
            };
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Password must not be empty."
            };
        }

        try
        {
            var builder = new OracleConnectionStringBuilder(_connectionString)
            {
                UserID = username,
                Password = password
            };

            await using var connection = new OracleConnection(builder.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            CurrentSession.SetLoggedInUser(username, builder.ConnectionString);

            return new LoginResult { Success = true };
        }
        catch (OracleException ex)
        {
            if (ex.Number == 1017) // ORA-01017: invalid username/password
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Sai username hoặc password."
                };
            }

            return new LoginResult
            {
                Success = false,
                ErrorMessage = $"Lỗi kết nối Oracle (code {ex.Number}): {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = $"Lỗi không xác định khi đăng nhập: {ex.Message}"
            };
        }
    }
}

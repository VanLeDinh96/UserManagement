using Oracle.ManagedDataAccess.Client;
using UserManagement.Extensions;
using UserManagement.Oracle;

namespace UserManagement.Repositories;

internal sealed class OracleUserAdminRepository(IOracleStoredProcExecutor executor) : IUserAdminRepository
{
    public async Task ChangePasswordAsync(string username, string newPassword)
    {
        var pUser = OracleParameterFactory.In("p_username", OracleDbType.Varchar2, username);
        var pPass = OracleParameterFactory.In("p_password", OracleDbType.Varchar2, newPassword);

        await executor.ExecuteNonQueryAsync(
            "user_admin_pkg.pro_alter_user",
            pUser,
            pPass);
    }

    public async Task CreateUserAsync(string username, string password)
    {
        var pUser = OracleParameterFactory.In("p_username", OracleDbType.Varchar2, username);
        var pPass = OracleParameterFactory.In("p_password", OracleDbType.Varchar2, password);

        await executor.ExecuteNonQueryAsync(
            "user_admin_pkg.pro_create_user",
            pUser,
            pPass);
    }

    public async Task CreateOrAlterUserAsync(string username, string password)
    {
        var pUser = OracleParameterFactory.In("p_username", OracleDbType.Varchar2, username);
        var pPass = OracleParameterFactory.In("p_password", OracleDbType.Varchar2, password);

        await executor.ExecuteNonQueryAsync(
            "user_admin_pkg.pro_create_or_alter_user",
            pUser,
            pPass);
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        var returnParam = OracleParameterFactory.Return(OracleDbType.Int32);

        var userParam = OracleParameterFactory.In(
            "p_username",
            OracleDbType.Varchar2,
            username);

        int result = await executor.ExecuteFunctionAsync<int>(
            "user_admin_pkg.fn_user_exists",
            returnParam,
            userParam);

        return result == 1;
    }
}

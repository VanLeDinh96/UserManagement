using UserManagement.Oracle;
using UserManagement.Repositories;
using UserManagement.Services;

namespace UserManagement.Extensions;

public static class CompositionRoot
{
    public static IUserAccountService BuildUserAccountService()
    {
        IOracleConnectionFactory connectionFactory = new OracleConnectionFactory();
        IOracleStoredProcExecutor executor = new OracleStoredProcExecutor(connectionFactory);
        IUserAdminRepository userAdminRepository = new OracleUserAdminRepository(executor);
        IUserAccountService userAccountService = new UserAccountService(userAdminRepository);
        return userAccountService;
    }

    public static ILoginService BuildLoginService()
    {
        ILoginService loginService = new OracleLoginService();
        return loginService;
    }
}

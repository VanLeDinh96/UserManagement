using UserManagement.Models;

namespace UserManagement.Services;

public interface ILoginService
{
    /// <summary>
    /// Try to authenticate a user against Oracle using the given username and password.
    /// </summary>
    Task<LoginResult> LoginAsync(string username, string password);
}

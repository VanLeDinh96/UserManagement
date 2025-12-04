namespace UserManagement.Services;

public interface IUserAccountService
{
    Task<bool> UserExistsAsync(string username);
    Task CreateOrAlterUserAsync(string username, string password);
    bool IsValidOracleIdentifier(string s);
}

using System.Text.RegularExpressions;
using UserManagement.Repositories;

namespace UserManagement.Services;

internal sealed class UserAccountService(IUserAdminRepository userAdminRepository) : IUserAccountService
{
    private readonly IUserAdminRepository _userAdminRepository = userAdminRepository
                               ?? throw new ArgumentNullException(nameof(userAdminRepository));

    public Task<bool> UserExistsAsync(string username)
        => _userAdminRepository.UserExistsAsync(username);

    public Task CreateOrAlterUserAsync(string username, string password)
        => _userAdminRepository.CreateOrAlterUserAsync(username, password);

    public bool IsValidOracleIdentifier(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        // Simple rule: start with letter, then letters/digits/underscore, max 30 chars
        return Regex.IsMatch(s, @"^[A-Za-z][A-Za-z0-9_]{0,29}$");
    }
}

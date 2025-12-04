namespace UserManagement.Repositories;

public interface IUserAdminRepository
{
    /// <summary>
    /// Check if a user exists in Oracle (via fn_user_exists).
    /// </summary>
    Task<bool> UserExistsAsync(string username);

    /// <summary>
    /// Call package to create or alter user (pro_create_or_alter_user).
    /// </summary>
    Task CreateOrAlterUserAsync(string username, string password);

    /// <summary>
    /// (Optional) Call only CREATE user procedure.
    /// </summary>
    Task CreateUserAsync(string username, string password);

    /// <summary>
    /// (Optional) Call only ALTER password procedure.
    /// </summary>
    Task ChangePasswordAsync(string username, string newPassword);
}

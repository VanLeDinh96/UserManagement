namespace UserManagement.Extensions;

public static class CurrentSession
{
    /// <summary>
    /// The currently logged-in Oracle username.
    /// </summary>
    public static string? Username { get; private set; }

    /// <summary>
    /// The Oracle connection string bound to the current user.
    /// </summary>
    public static string? UserConnectionString { get; private set; }

    /// <summary>
    /// Set the logged-in user information.
    /// </summary>
    public static void SetLoggedInUser(string username, string connectionString)
    {
        Username = username;
        UserConnectionString = connectionString;
    }

    /// <summary>
    /// Clear session data (for logout).
    /// </summary>
    public static void Clear()
    {
        Username = null;
        UserConnectionString = null;
    }
}

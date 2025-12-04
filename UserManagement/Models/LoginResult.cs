namespace UserManagement.Models;

public sealed record LoginResult
{
    /// <summary>
    /// Indicates whether the login was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Human-readable error message (for UI), empty on success.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;
}

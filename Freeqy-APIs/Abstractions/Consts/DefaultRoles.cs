namespace Freeqy_APIs.Abstractions.Consts;

/// <summary>
/// Centralised role name constants. Use these everywhere instead of magic strings.
/// </summary>
public static class DefaultRoles
{
    public const string Admin = "Admin";
    public const string User  = "User";

    /// <summary>All roles that must exist in the database.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, User];
}

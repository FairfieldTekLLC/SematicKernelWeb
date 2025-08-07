using System.Security.Claims;
using System.Security.Principal;

namespace SematicKernelWeb.ClaimsIdentities;

public static class PrincipalExtensions
{
    public static UserIdentity? GetUserIdentity(this IPrincipal principal)
    {
        return (principal as ClaimsPrincipal)?.Identities.OfType<UserIdentity>().FirstOrDefault();
    }

    public static string? GetDisplayName(this IPrincipal principal)
    {
        UserIdentity? identity = principal.GetUserIdentity();
        if (identity == null)
            return "Warning: Authentication is not turned on.";

        return identity.Name;
    }

    public static Guid GetId(this IPrincipal principal)
    {
        UserIdentity? identity = principal.GetUserIdentity();

        return identity?.Id ?? Guid.Empty;
    }
}
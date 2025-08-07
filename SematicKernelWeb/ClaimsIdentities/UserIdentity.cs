using System.Security.Claims;
using SematicKernelWeb.Models;

namespace SematicKernelWeb.ClaimsIdentities;

public class UserIdentity(Securityobject user) : ClaimsIdentity(SetClaims(user))
{
    public static string EarsClaimType = "NewsReader Claim";

    public Guid Id { get; } = user.Activedirectoryid;

    public string? FullName { get; } = user.Fullname;

    public string? Email { get; } = user.Emailaddress;

    //public IUserPreferences Preferences { get; }

    public bool HasPermission(string permission)
    {
        return HasClaim(EarsClaimType, permission);
    }

    private static List<Claim> SetClaims(Securityobject user)
    {
        List<Claim> claims = new();

        //foreach (dcPermission permission in user.Permissions)
        //{
        //    claims.Add(new Claim(EarsClaimType, permission.Name));
        //}

        return claims;
    }
}
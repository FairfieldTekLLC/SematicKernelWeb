using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using SematicKernelWeb.ClaimsIdentities;
using SematicKernelWeb.Models;

namespace SematicKernelWeb.Middleware;

public class AddIdentityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        WindowsIdentity? user = context.User.Identity as WindowsIdentity;

        try
        {
            using DirectoryEntry userDirectoryEntry = new($"LDAP://<SID={user?.User}>");
            using PrincipalContext principalContext = new(ContextType.Domain);
            UserPrincipal userPrincipal =
                UserPrincipal.FindByIdentity(principalContext, userDirectoryEntry.Guid.ToString());

            if (userPrincipal != null)
            {
                await using NewsReaderContext ctx = new NewsReaderContext();
                Securityobject? dat = ctx.Securityobjects.FirstOrDefault(x =>
                    x.Activedirectoryid == userPrincipal.Guid);

                if (dat == null)
                {
                    dat = new Securityobject
                    {
                        Activedirectoryid = userPrincipal.Guid.Value,
                        Emailaddress = userPrincipal.EmailAddress,
                        Forename = userPrincipal.GivenName,
                        Surname = userPrincipal.Surname,
                        Fullname = userPrincipal.DisplayName,
                        Isactive = 1,
                        Isgroup = 0,
                        Username = userPrincipal.SamAccountName,
                        Pass = ""
                    };
                    ctx.Securityobjects.Add(dat);
                    await ctx.SaveChangesAsync();
                }

                if (dat != null)
                    context.User.AddIdentity(new UserIdentity(dat));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        // Call the next delegate/middleware in the pipeline.
        await next(context);
    }
}
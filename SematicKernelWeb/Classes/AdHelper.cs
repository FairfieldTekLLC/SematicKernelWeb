using System.DirectoryServices.AccountManagement;
//using SematicKernelWeb.ClaimsIdentities;
using Microsoft.AspNetCore.StaticFiles;

namespace SematicKernelWeb.Classes;

public static class AdHelper
{
    public static string GetUserName(this Guid activeDirectoryId)
    {
        try
        {
            using PrincipalContext principalContext = new(ContextType.Domain);
            UserPrincipal userPrincipal =
                UserPrincipal.FindByIdentity(principalContext, activeDirectoryId.ToString());

            if (userPrincipal != null)
                return userPrincipal.SamAccountName ?? userPrincipal.UserPrincipalName ?? userPrincipal.Name;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return "";
    }
    

    public static string GetMimeTypeForFileExtension(this string filePath)
    {
        const string DefaultContentType = "application/octet-stream";

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string contentType))
        {
            contentType = DefaultContentType;
        }

        return contentType;
    }
}
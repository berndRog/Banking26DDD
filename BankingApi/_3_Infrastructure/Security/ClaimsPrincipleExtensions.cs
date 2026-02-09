namespace BankingApi._3_Infrastructure.Security;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
   public static int GetAdminRights(this ClaimsPrincipal user)
   {
      var raw = user.FindFirst("admin_rights")?.Value;
      return int.TryParse(raw, out var rights) ? rights : 0;
   }

   public static bool HasAdminRight(this ClaimsPrincipal user, int required)
      => (user.GetAdminRights() & required) == required;
}

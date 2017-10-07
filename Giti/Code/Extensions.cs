using System;
using System.Security.Claims;

namespace Giti.Code
{
    public static class Extensions
    {
		public static string GetUserId(this ClaimsPrincipal user)
		{
			if (!user.Identity.IsAuthenticated)
				return null;

			ClaimsPrincipal currentUser = user;
			return currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
		}
    }
}

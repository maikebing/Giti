using System;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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

        public static string GetReposBaseFolderPath(this IHostingEnvironment hostingEnvironment)
        {
            return $"{hostingEnvironment.ContentRootPath}/repos";
        }

		public static string UnencodedRouteLink(this IUrlHelper helper, string routeName, object routeValues)
		{
			return WebUtility.UrlDecode(helper.RouteUrl(routeName, routeValues));
		}
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CelebrateHubMVC.Filters
{
    public class RequireSessionTokenFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            // Only check for users who appear authenticated via cookie
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var token = httpContext.Session.GetString("JWT");

                if (string.IsNullOrEmpty(token))
                {
                    // Cookie says "logged in" but session has no token —
                    // force a clean logout + redirect to login.
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    httpContext.Session.Clear();

                    var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
                    context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                        $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}&expired=true");
                    return;
                }
            }

            await next();
        }
    }
}
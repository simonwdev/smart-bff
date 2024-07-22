using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SmartBff.Extensions;

namespace SmartBff.Middleware.CookieEvents;

/// <summary>
/// Overrides cookie behaviour to be suitable for use with
/// single page JavaScript application.
/// </summary>
public class SinglePageCookieAuthenticationEvents : CookieAuthenticationEvents
{
    public override async Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        await context.HttpContext.Response.WriteProblemAsync(HttpStatusCode.Unauthorized, "Log in required.");
        
        // Note we don't call base.RedirectToLogin as we wish to suppress that behaviour.
    }
    
    public override async Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        await context.HttpContext.Response.WriteProblemAsync(HttpStatusCode.Unauthorized, "Access denied.");
        
        // Note we don't call base.RedirectToAccessDenied as we wish to suppress that behaviour.
    }

    public override async Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
    {
        await context.HttpContext.Response.WriteProblemAsync(HttpStatusCode.Unauthorized, "Logged out.");
        
        // Note we don't call base.RedirectToLogout as we wish to suppress that behaviour.
    }
}
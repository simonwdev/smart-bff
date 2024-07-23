using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using SmartBff.Extensions;

namespace SmartBff.Middleware.CookieEvents;

/// <summary>
/// Overrides cookie behaviour with bff session logic.
/// </summary>
public class SessionCookieAuthenticationEvents(ILogger<SessionCookieAuthenticationEvents> logger) : SinglePageCookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        if (TimeSpan.TryParse(context.Properties.GetString(Constants.AuthenticationProperties.SessionSlidingDuration), out var duration))
        {
            // Set the cookie & ticket duration to the configured duration.
            // This allows the session to last for as long as the refresh token is valid.
            context.CookieOptions.MaxAge = duration;
            context.Options.ExpireTimeSpan = duration;
        }
        
        context.Properties.SetString(Constants.AuthenticationProperties.TicketCreatedOnTicks, DateTime.UtcNow.Ticks.ToString());

        await base.SigningIn(context);
    }
    
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var ticketIssuedTicksValue = context.Properties.GetString(Constants.AuthenticationProperties.TicketCreatedOnTicks);

        if (ticketIssuedTicksValue is null || 
            !long.TryParse(ticketIssuedTicksValue, out var ticketIssuedTicks) ||
            !TimeSpan.TryParse(context.Properties.GetString(Constants.AuthenticationProperties.SessionMaxDuration), out var maxDuration))
        {
            logger.SessionStateInvalid(context.Principal?.Identity?.AuthenticationType ?? "n/a");
            await RejectPrincipalAsync(context);
            return;
        }

        var ticketIssuedUtc = new DateTime(ticketIssuedTicks);

        // Reject the principal if the maximum lifetime is exceeded.
        if (DateTime.UtcNow - ticketIssuedUtc > maxDuration)
        {
            logger.SessionMaxDurationExceeded(context.Principal?.Identity?.AuthenticationType ?? "n/a");
            await RejectPrincipalAsync(context);
            return;
        }

        await base.ValidatePrincipal(context);
    }
    
    private static async Task RejectPrincipalAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();

        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }
}
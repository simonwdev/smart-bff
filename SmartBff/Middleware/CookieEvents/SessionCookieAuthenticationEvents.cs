using Microsoft.AspNetCore.Authentication.Cookies;

namespace SmartBff.Middleware.CookieEvents;

/// <summary>
/// Overrides cookie behaviour with bff session logic.
/// </summary>
public class SessionCookieAuthenticationEvents : SinglePageCookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        if (TimeSpan.TryParse(context.Properties.GetString(Constants.AuthenticationProperties.RefreshTokenDuration), out var duration))
        {
            // Set the cookie & ticket duration to the configured duration.
            // This allows the session to last for as long as the refresh token is valid.
            context.CookieOptions.MaxAge = duration;
            context.Options.ExpireTimeSpan = duration;
        }

        await base.SigningIn(context);
    }
}
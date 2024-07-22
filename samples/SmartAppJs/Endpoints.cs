using System.Security.Claims;
using SmartBff;
using SmartBff.Extensions;

namespace SmartAppJs;

public static class Endpoints
{
    public static async Task<IResult> GetExternalService(ClaimsPrincipal principal)
    {
        // This API call is protected by the session cookie.
        // It could use client credential flows to access other services.
        // This wouldn't be possible using a normal SPA JavaScript app.
    
        var registrationId = principal.GetRequiredClaimValue(Constants.CustomClaims.RegistrationId);

        if (registrationId != "1")
            return Results.Forbid(authenticationSchemes: new[] { Constants.AuthenticationSchemes.Session });
    
        // Simulate some external protected system.
        await Task.Delay(1000);
        
        var payload = new { Value = Guid.NewGuid().ToString() };
        
        return Results.Json(payload);
    }
}
using System.Net;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SmartBff.Extensions;
using SmartBff.ProblemDetails;

namespace SmartBff.Endpoints.Logout;

public static class LogoutApi
{   
    public static async Task LogoutAsync(HttpContext httpContext, [AsParameters] EndpointServices services)
    {
        var returnUrl = httpContext.Request.Query[Constants.QueryParameters.ReturnUrlQueryParameter].FirstOrDefault() ?? "/";
        
        // Only allow local return URLs to prevent open redirect attacks.
        if (!UrlHelper.IsLocal(returnUrl))
        {
            services.Logger.InvalidReturnUrl(returnUrl);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest, "Return URL is not valid.");
            return;
        }

        var loginResult = await httpContext.AuthenticateAsync(Constants.AuthenticationSchemes.Session);
        if (loginResult is null or { Succeeded: false })
        {
            // If we can't find the session it means the client
            // might have removed the cookie.
            httpContext.Response.Redirect(returnUrl);
            services.Logger.LogoutRequestSuccess(returnUrl);
            
            return;
        }

        // Extract the required claims from the login token.
        var cookieRegistrationId = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.RegistrationId);
        var accessTokenValue = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.AccessToken);
        var refreshTokenValue = loginResult.Principal.GetOptionalClaimValue(Constants.CustomClaims.RefreshToken);
        
        var registration = services.Options.Value.GetRegistrationById(cookieRegistrationId) 
            ?? throw new InvalidOperationException("Unable to find registration.");
        
        var documentResponse = await services.SmartConfigurationService.GetDiscoveryDocumentAsync(registration);
        if (documentResponse is { IsValid: false })
        {
            services.Logger.InvalidSmartConfiguration(documentResponse.ValidationErrors.Join("|"));
            
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Session);
            await httpContext.Response.WriteProblemAsync(new SmartDocumentProblemDetails(documentResponse));
            return;
        }
        
        if (registration.Options.RevokeOnLogout && !string.IsNullOrWhiteSpace(documentResponse.Configuration.RevocationEndpoint))
        {
            var tokens = new List<RevocationToken>() { new(accessTokenValue, OidcConstants.TokenTypes.AccessToken) };
            if (!string.IsNullOrWhiteSpace(refreshTokenValue))
                tokens.Add(new RevocationToken(refreshTokenValue, OidcConstants.TokenTypes.RefreshToken));
            
            using var httpClient = services.HttpClientFactory.CreateClient();

            var responses = await httpClient.RevokeTokensAsync(registration, 
                new Uri(documentResponse.Configuration.RevocationEndpoint, UriKind.Absolute),
                tokens.ToArray());

            // Log the response.
            // We ignore errors as there is nothing the
            // client can do. 
            foreach (var response in responses)
            {
                if (response.TokenRevocationesponse.IsError)
                    services.Logger.TokenRevocationFailed(registration.RegistrationId, 
                        response.RevocationToken.TokenType, 
                        response.TokenRevocationesponse.Error ?? "n/a");
                else
                    services.Logger.TokenRevocationComplete(registration.RegistrationId, response.RevocationToken.TokenType);
            }
        }
        
        await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Session);
        
        httpContext.Response.Redirect(returnUrl);
        
        services.Logger.LogoutRequestSuccess(returnUrl);
    }
}
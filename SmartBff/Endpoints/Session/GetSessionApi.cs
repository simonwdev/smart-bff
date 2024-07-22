using System.Security.Claims;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SmartBff.Extensions;
using SmartBff.ProblemDetails;

namespace SmartBff.Endpoints.Session;

public class GetSessionApi
{
    public static async Task GetSessionAsync(HttpContext httpContext, [AsParameters] EndpointServices services)
    {
        var sessionResult = await httpContext.AuthenticateAsync(Constants.AuthenticationSchemes.Session);
        if (sessionResult is null or { Succeeded: false })
        {
            // We just return 401, it's an expected outcome to indicate to
            // the client they are not logged in.
            httpContext.Response.StatusCode = 401;
            return;
        }

        var principal = sessionResult.Principal; 
        var cookieName = principal.GetRequiredClaimValue(JwtClaimTypes.Name);
        var cookieRegistrationId = principal.GetRequiredClaimValue(Constants.CustomClaims.RegistrationId);
        var cookieAccessTokenValue = principal.GetRequiredClaimValue(Constants.CustomClaims.AccessToken);
        var cookieRefreshToken = principal.GetOptionalClaimValue(Constants.CustomClaims.RefreshToken);
        var accessToken = JwtExtensions.ReadToken(cookieAccessTokenValue);

        // Use the refresh token to get a new access token
        // if we have expired or are close to expiry.
        if (cookieRefreshToken is not null 
            && accessToken.IsExpiredOrExpiring(DateTime.UtcNow, services.Options.Value.AccessTokenExpiryThresholdPercentage))
        {
            var registration = services.Options.Value.GetRegistrationById(cookieRegistrationId) 
                ?? throw new InvalidOperationException("Unable to find registration.");
            
            var documentResponse = await services.SmartConfigurationService.GetDiscoveryDocumentAsync(registration);
            if (documentResponse is { IsValid: false })
            {
                services.Logger.InvalidSmartConfiguration(documentResponse.ValidationErrors.Join("|"));
                await httpContext.Response.WriteProblemAsync(new SmartDocumentProblemDetails(documentResponse));
                return;
            }
            
            using var httpClient = services.HttpClientFactory.CreateClient();

            var tokenClient = new TokenClient(httpClient, new TokenClientOptions
            {
                ClientId = registration.ClientId,
                ClientSecret = registration.ClientSecret,
                Address = documentResponse.Configuration.TokenEndpoint ?? string.Empty
            });
            
            // If the authorisation server rejects our token request
            // we and remove the session cookie and return the error to the client.
            var tokenResponse = await tokenClient.RequestRefreshTokenAsync(cookieRefreshToken);
            if (tokenResponse is { IsError: true } or { AccessToken: null })
            { 
                services.Logger.TokenRequestFailed(tokenResponse.Error, tokenResponse.ErrorDescription);
                await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Session);
                await httpContext.Response.WriteProblemAsync(new TokenResponseProblemDetails(tokenResponse));
                return;
            }

            // Re-create the session cookie to store the access, id, & refresh token.
            principal = new ClaimsPrincipal(registration.CreateIdentityFromTokenResponse(tokenResponse));
            
            await httpContext.SignInAsync(Constants.AuthenticationSchemes.Session, principal);
        }
        
        var latestIdTokenValue = principal.GetOptionalClaimValue(Constants.CustomClaims.IdentityToken);
        var latestAccessTokenValue = principal.GetRequiredClaimValue(Constants.CustomClaims.AccessToken);
        
        var details = new GetSessionResponse(name: cookieName,
            accessToken: latestAccessTokenValue,
            idToken: latestIdTokenValue);
        
        await httpContext.Response.WriteAsJsonAsync(details);
        
        services.Logger.SessionRequestSuccess(cookieRegistrationId);
    }
}
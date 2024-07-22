using System.Net;
using System.Security.Claims;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SmartBff.Extensions;
using SmartBff.ProblemDetails;

namespace SmartBff.Endpoints.LoginCallback;

public static class LoginCallbackApi
{
    public static async Task LoginCallbackAsync(HttpContext httpContext, [AsParameters] EndpointServices services)
    {
        var code = httpContext.Request.Query[OidcConstants.AuthorizeResponse.Code].FirstOrDefault();
        var state = httpContext.Request.Query[OidcConstants.AuthorizeResponse.State].FirstOrDefault();
        var error = httpContext.Request.Query[OidcConstants.AuthorizeResponse.Error].FirstOrDefault();

        // Handle error responses from the authorisation server.
        // https://www.rfc-editor.org/rfc/rfc6749.html#section-4.1.2.1
        // We don't check the 'state' parameter as we are rejecting the request anyway.
        if (!string.IsNullOrWhiteSpace(error))
        {
            var errorDescription = httpContext.Request.Query[OidcConstants.AuthorizeResponse.ErrorDescription].FirstOrDefault();
            
            services.Logger.AuthorisationCallbackFailed(error, errorDescription);
            
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(new AuthorizeCallbackProblemDetails(error, errorDescription));
            
            return;
        }
        
        // Code is mandatory and state is required as we always send it on the request.
        // https://www.rfc-editor.org/rfc/rfc6749.html#section-4.1.2
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            services.Logger.NoParameter(httpContext.Request.Path, "code|state");
            
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest, "Invalid callback parameters.");
            return;
        }

        // Try to find the login cookie. If the cookie cannot be found, this may indicate that the
        // authorization response is unsolicited and potentially malicious or be caused by an invalid
        // or inadequate same-site configuration.
        // In any case, the authentication demand MUST be rejected as it's impossible to ensure
        // it's not an injection or session fixation attack without the correlation cookie.
        var loginResult = await httpContext.AuthenticateAsync(Constants.AuthenticationSchemes.Login);
        if (loginResult is null or { Succeeded: false })
        {
            services.Logger.NoCookie(Constants.AuthenticationSchemes.Login);
            
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest, "Invalid login session.");
            return;
        }

        // Extract the required claims from the login token.
        var cookieRegistrationId = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.RegistrationId);
        var cookieLoginCallback = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.LoginCallback);
        var cookiePkceCodeVerifier = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.PkceCodeVerifier);
        var cookieState = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.State);
        var cookieReturnUrl = loginResult.Principal.GetRequiredClaimValue(Constants.CustomClaims.ReturnUrl);

        // If the registration does not exist it suggests buggy cookie code
        // or a configuration change during the login flow.
        // Either way, we MUST reject the authentication demand.
        var registration = services.Options.Value.GetRegistrationById(cookieRegistrationId) 
            ?? throw new InvalidOperationException("Unable to find registration.");
        
        var documentResponse = await services.SmartConfigurationService.GetDiscoveryDocumentAsync(registration);
        if (documentResponse is { IsValid: false })
        {
            services.Logger.InvalidSmartConfiguration(documentResponse.ValidationErrors.Join("|"));
            
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(new SmartDocumentProblemDetails(documentResponse));
            return;
        }
        
        if (!cookieState.Equals(state, StringComparison.Ordinal))
        {
            // The state parameter does not match our login cookie value.
            // This might indicate an authorization code injection attack.
            // We MUST reject the authentication demand.
            services.Logger.InvalidStateParameter(cookieState, state);
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.Unauthorized, "Invalid state.");
            return;
        }

        using var httpClient = services.HttpClientFactory.CreateClient();

        var tokenClient = new TokenClient(httpClient, new TokenClientOptions
        {
            ClientId = registration.ClientId,
            ClientSecret = registration.ClientSecret,
            Address = documentResponse.Configuration.TokenEndpoint ?? string.Empty
        });

        var tokenResponse = await tokenClient.RequestAuthorizationCodeTokenAsync(code,
            cookieLoginCallback,
            cookiePkceCodeVerifier);

        // If the authorisation server rejects our token request we pass this on to the client.
        if (tokenResponse is { IsError: true } or { AccessToken: null })
        { 
            services.Logger.AuthorisationCodeTokenRequestFailed(tokenResponse.Error, tokenResponse.ErrorDescription);
            await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
            await httpContext.Response.WriteProblemAsync(new TokenResponseProblemDetails(tokenResponse));
            return;
        }

        // Pass properties through to the cookie creation pipeline.
        var properties = new Dictionary<string, string?>
        {
            { Constants.AuthenticationProperties.RegistrationId, registration.RegistrationId },
            { Constants.AuthenticationProperties.RefreshTokenDuration, registration.Options.RefreshTokenDuration.ToString() }
        };

        // Create the session cookie to store the access, id, & refresh token.
        await httpContext.SignInAsync(Constants.AuthenticationSchemes.Session, 
            new ClaimsPrincipal(registration.CreateIdentityFromTokenResponse(tokenResponse)), 
            new AuthenticationProperties(properties));
        
        // Delete the login cookie to avoid a replay attack.
        await httpContext.SignOutAsync(Constants.AuthenticationSchemes.Login);
        
        httpContext.Response.Redirect(cookieReturnUrl);
        
        services.Logger.LoginCallbackRequestSuccess(httpContext.Request.Path);
    }
}
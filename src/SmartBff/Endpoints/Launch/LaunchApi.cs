using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SmartBff.Extensions;
using SmartBff.ProblemDetails;

namespace SmartBff.Endpoints.Launch;

public class LaunchApi
{
    public static async Task LaunchAsync(HttpContext httpContext, [AsParameters] EndpointServices services)
    {
        var issuer = httpContext.Request.Query[Constants.QueryParameters.IssuerQueryParameter].FirstOrDefault();
        var launchValue = httpContext.Request.Query[Constants.QueryParameters.LaunchQueryParameter].FirstOrDefault();
        var returnUrl = httpContext.Request.Query[Constants.QueryParameters.ReturnUrlQueryParameter].FirstOrDefault() ?? "/";
        var discriminator = httpContext.Request.Query[Constants.QueryParameters.DiscriminatorQueryParameter].FirstOrDefault();

        var options = services.Options.Value;
        
        // Issuer is mandatory and must be a URL.
        if (string.IsNullOrWhiteSpace(issuer) || !UrlHelper.IsAbsoluteUrl(issuer))
        {
            services.Logger.NoParameter(httpContext.Request.Path, Constants.QueryParameters.IssuerQueryParameter);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest, "Issuer must be a valid URL.");
            return;
        }

        // Only allow local return URLs to prevent open redirect attacks.
        if (!UrlHelper.IsLocal(returnUrl))
        {
            services.Logger.InvalidReturnUrl(returnUrl);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest, "Return URL is not valid.");
            return;
        }

        var registration = options.AllowLaunchDiscriminator && !string.IsNullOrWhiteSpace(discriminator) 
            ? options.GetRegistrationByIssuerAndDiscriminator(issuer, discriminator) 
            : options.GetRegistrationByIssuer(issuer); 
        
        if (registration is null)
        {
            services.Logger.RegistrationLookupFailed(httpContext.Request.Path, issuer);
            await httpContext.Response.WriteProblemAsync(HttpStatusCode.BadRequest);
            return;
        }
        
        var documentResponse = await services.SmartConfigurationService.GetDiscoveryDocumentAsync(registration);
        if (documentResponse is { IsValid: false })
        {
            services.Logger.InvalidSmartConfiguration(documentResponse.ValidationErrors.Join("|"));
            await httpContext.Response.WriteProblemAsync(new SmartDocumentProblemDetails(documentResponse));
            return;
        }
        
        // PKCE is mandatory with S256 challenge method.
        // https://build.fhir.org/ig/HL7/smart-app-launch/app-launch.html#considerations-for-pkce-support
        var codeVerifier = CryptoRandom.CreateUniqueId();
        var codeChallenge = Base64Url.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));

        var stateNonce = CryptoRandom.CreateUniqueId();
        var endpointUrl = new RequestUrl(documentResponse.Configuration.AuthorizationEndpoint ?? string.Empty);

        var customParameters = new Parameters();
        customParameters.AddOptional(Constants.QueryParameters.LaunchQueryParameter, launchValue);

        var authorizeUrl = endpointUrl.CreateAuthorizeUrl(clientId: registration.ClientId,
            responseType: OidcConstants.ResponseTypes.Code,
            redirectUri: registration.LoginCallbackUrl,
            scope: registration.Scopes,
            state: stateNonce,
            codeChallenge: codeChallenge,
            codeChallengeMethod: OidcConstants.CodeChallengeMethods.Sha256,
            extra: customParameters);

        var loginIdentity = new ClaimsIdentity(Constants.AuthenticationSchemes.Login);
        loginIdentity.AddClaim(Constants.CustomClaims.RegistrationId, registration.RegistrationId);
        loginIdentity.AddClaim(Constants.CustomClaims.LoginCallback, registration.LoginCallbackUrl);
        loginIdentity.AddClaim(Constants.CustomClaims.ReturnUrl, returnUrl);
        loginIdentity.AddClaim(Constants.CustomClaims.PkceCodeVerifier, codeVerifier);
        loginIdentity.AddClaim(Constants.CustomClaims.State, stateNonce);

        // Set the login cookie to store the details of the authorisation code flow.
        await httpContext.SignInAsync(Constants.AuthenticationSchemes.Login, new ClaimsPrincipal(loginIdentity));

        // Redirect the browser to the authorisation url.
        httpContext.Response.Redirect(authorizeUrl);
        
        services.Logger.LaunchRequestSuccess(issuer);
    }
}
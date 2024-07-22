using System.Security.Claims;
using System.Text.Json.Serialization;
using IdentityModel;
using IdentityModel.Client;
using SmartBff.Extensions;

namespace SmartBff.Configuration;

public class Registration
{
    // Parametrized constructor for IConfiguration/Json binding.
    [JsonConstructor]
    public Registration(
        string? registrationId = null,
        string? discriminator = null,
        string? clientId = null,
        string? clientSecret = null,
        string? issuer = null,
        string? metadataAddress = null,
        string? scopes = null,
        string? loginCallbackUrl = null,
        bool? active = null,
        RegistrationOptions? options = null)
    {
        RegistrationId = registrationId ?? string.Empty;
        Discriminator = discriminator;
        ClientId = clientId ?? string.Empty;
        ClientSecret = clientSecret ?? string.Empty;
        Issuer = issuer ?? string.Empty;
        MetadataAddress = metadataAddress ?? string.Empty;
        Scopes = scopes ?? string.Empty;
        LoginCallbackUrl = loginCallbackUrl ?? string.Empty;
        Active = active ?? false;
        Options = options ?? new RegistrationOptions();
    }
    
    /// <summary>
    /// An optional value that can be used to launch registrations
    /// that have the same issuer.
    /// The value could either be unique or a value such as 'dev', 'test', etc.
    /// </summary>
    public string? Discriminator { get; set; }
    public string RegistrationId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Issuer { get; set; }
    public string MetadataAddress { get; set; }
    public string Scopes { get; set; }
    public string LoginCallbackUrl { get; set; }
    public bool Active { get; set; }
    public RegistrationOptions Options { get; set; }
    
    public ClaimsIdentity CreateIdentityFromTokenResponse(TokenResponse tokenResponse)
    {
        ArgumentNullException.ThrowIfNull(tokenResponse);
        
        if (tokenResponse is { AccessToken: null })
            throw new InvalidOperationException("Access token must be provided.");

        var accessToken = JwtExtensions.ReadToken(tokenResponse.AccessToken);
        var idToken = tokenResponse.IdentityToken is not null ? JwtExtensions.ReadToken(tokenResponse.IdentityToken) : null;

        var identity = new ClaimsIdentity(
            authenticationType: Constants.AuthenticationSchemes.Session,
            nameType: JwtClaimTypes.Name,
            roleType: JwtClaimTypes.Role);

        // Fallback to the access token 'sub' if there is no identity token.
        identity.AddClaim(JwtClaimTypes.Name, idToken is not null
            ? idToken.GetRequiredClaimValue(JwtClaimTypes.Name)
            : accessToken.GetRequiredClaimValue(JwtClaimTypes.Subject));

        identity.AddClaim(Constants.CustomClaims.RegistrationId, RegistrationId);
        identity.AddClaim(Constants.CustomClaims.AccessToken, tokenResponse.AccessToken);
        
        if (tokenResponse.IdentityToken is not null)
            identity.AddClaim(Constants.CustomClaims.IdentityToken, tokenResponse.IdentityToken);
        
        if (tokenResponse.RefreshToken is not null)
            identity.AddClaim(Constants.CustomClaims.RefreshToken, tokenResponse.RefreshToken);

        return identity;
    }
}
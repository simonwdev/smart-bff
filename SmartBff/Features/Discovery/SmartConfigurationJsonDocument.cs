using System.Text.Json;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using SmartBff.Extensions;

namespace SmartBff.Features.Discovery;

/// <summary>
/// Json representation of the .well-known/smart-configuration metadata document.
/// Uses <see cref="System.Text.Json.JsonDocument"/> to allow fined-grained control over parsing.
/// </summary>
public class SmartConfigurationJsonDocument(JsonDocument jsonDocument) : IDisposable
{
    public JsonDocument JsonDocument => jsonDocument;
    public JsonElement JsonElement { get; } = jsonDocument.RootElement;
    public string? Issuer { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.Issuer);
    public string? JwksUri { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.JwksUri);
    public string? AuthorizationEndpoint { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.AuthorizationEndpoint);
    public string? TokenEndpoint { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.TokenEndpoint);

    public List<string>? TokenEndpointAuthMethodsSupported { get; } =
        jsonDocument.RootElement.TryGetStringArrayOrNull(OidcConstants.Discovery.TokenEndpointAuthenticationMethodsSupported);

    public List<string>? GrantTypesSupported { get; } = jsonDocument.RootElement.TryGetStringArrayOrNull(OidcConstants.Discovery.GrantTypesSupported);
    public string? RegistrationEndpoint { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.RegistrationEndpoint);
    public List<string>? ScopesSupported { get; } = jsonDocument.RootElement.TryGetStringArrayOrNull(OidcConstants.Discovery.ScopesSupported);
    public List<string>? ResponseTypesSupported { get; } = jsonDocument.RootElement.TryGetStringArrayOrNull(OidcConstants.Discovery.ResponseTypesSupported);
    public string? ManagementEndpoint { get; } = jsonDocument.RootElement.GetString(Constants.Discovery.ManagementEndpoint);
    public string? IntrospectionEndpoint { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.IntrospectionEndpoint);
    public string? RevocationEndpoint { get; } = jsonDocument.RootElement.GetString(OidcConstants.Discovery.RevocationEndpoint);
    public List<string>? CodeChallengeMethodsSupported { get; } = jsonDocument.RootElement.TryGetStringArrayOrNull(OidcConstants.Discovery.CodeChallengeMethodsSupported);
    public List<string>? Capabilities { get; } = jsonDocument.RootElement.TryGetStringArrayOrNull(Constants.Discovery.Capabilities);

    public List<AssociatedEndpoint>? AssociatedEndpoints { get; } = jsonDocument.RootElement.TryGetObjectArrayOrNull(Constants.Discovery.AssociatedEndpoints,
        (e) => new AssociatedEndpoint(e.GetString(Constants.Discovery.AssociatedEndpointsUrl), e.TryGetStringArrayOrNull(Constants.Discovery.Capabilities)));

    public void Dispose()
    {
        jsonDocument.Dispose();
    }
}
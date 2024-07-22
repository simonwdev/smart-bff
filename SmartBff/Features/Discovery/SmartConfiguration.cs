namespace SmartBff.Features.Discovery;

/// <summary>
/// Structured representation of the .well-known/smart-configuration metadata document.
/// </summary>
public class SmartConfiguration
{
    public static readonly SmartConfiguration Empty = new();

    public SmartConfiguration()
    {
        TokenEndpointAuthMethodsSupported = [];
        GrantTypesSupported = [];
        ScopesSupported = [];
        ResponseTypesSupported = [];
        CodeChallengeMethodsSupported = [];
        Capabilities = [];
        AssociatedEndpoints = [];
    }

    public SmartConfiguration(
        string? issuer,
        string? jwksUri,
        string? authorizationEndpoint,
        string? tokenEndpoint,
        List<string>? tokenEndpointAuthMethodsSupported,
        List<string>? grantTypesSupported,
        string? registrationEndpoint,
        List<string>? scopesSupported,
        List<string>? responseTypesSupported,
        string? managementEndpoint,
        string? introspectionEndpoint,
        string? revocationEndpoint,
        List<string>? codeChallengeMethodsSupported,
        List<string>? capabilities,
        List<AssociatedEndpoint>? associatedEndpoints
    )
    {
        Issuer = issuer;
        JwksUri = jwksUri;
        AuthorizationEndpoint = authorizationEndpoint;
        TokenEndpoint = tokenEndpoint;
        TokenEndpointAuthMethodsSupported = tokenEndpointAuthMethodsSupported ?? [];
        GrantTypesSupported = grantTypesSupported ?? [];
        RegistrationEndpoint = registrationEndpoint;
        ScopesSupported = scopesSupported ?? [];
        ResponseTypesSupported = responseTypesSupported ?? [];
        ManagementEndpoint = managementEndpoint;
        IntrospectionEndpoint = introspectionEndpoint;
        RevocationEndpoint = revocationEndpoint;
        CodeChallengeMethodsSupported = codeChallengeMethodsSupported ?? [];
        Capabilities = capabilities ?? [];
        AssociatedEndpoints = associatedEndpoints ?? [];
    }

    public string? Issuer { get; }
    public string? JwksUri { get; }
    public string? AuthorizationEndpoint { get; }
    public string? TokenEndpoint { get; }
    public IReadOnlyList<string>? TokenEndpointAuthMethodsSupported { get; }
    public IReadOnlyList<string>? GrantTypesSupported { get; }
    public string? RegistrationEndpoint { get; }
    public IReadOnlyList<string>? ScopesSupported { get; }
    public IReadOnlyList<string>? ResponseTypesSupported { get; }
    public string? ManagementEndpoint { get; }
    public string? IntrospectionEndpoint { get; }
    public string? RevocationEndpoint { get; }
    public IReadOnlyList<string>? CodeChallengeMethodsSupported { get; }
    public IReadOnlyList<string>? Capabilities { get; }
    public IReadOnlyList<AssociatedEndpoint>? AssociatedEndpoints { get; }
}
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartBff.Configuration;
using SmartBff.Extensions;

namespace SmartBff.Features.Discovery;

public class SmartConfigurationService(
    IOptions<SmartBffOptions> options,
    IHttpClientFactory httpClientFactory, 
    ISmartConfigurationCache smartConfigurationCache,
    ISmartConfigurationValidator validator,
    ILogger<SmartConfigurationService> logger) : ISmartConfigurationService
{
    public async Task<SmartConfigurationDocumentResponse> GetDiscoveryDocumentAsync(Registration registration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var uri = new Uri(registration.MetadataAddress, UriKind.Absolute);

        // We expect the registration to be validated at this point.
        // Just perform basic sanity checks.
        if (!UrlHelper.IsAbsoluteUrl(uri))
            throw new InvalidOperationException("Configuration is not valid.");
        
        if (smartConfigurationCache.Cache.TryGetValue(uri, out SmartConfiguration? smartConfiguration))
        {
            ArgumentNullException.ThrowIfNull(smartConfiguration);
            
            logger.CacheHit(nameof(smartConfigurationCache), uri.ToString());
            
            return new SmartConfigurationDocumentResponse(smartConfiguration);
        }
        
        logger.CacheMiss(nameof(smartConfigurationCache), uri.ToString());
        
        using var client = httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage();
        request.RequestUri = uri;
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        // The expectation is the smart config is reasonably small, so it is fine to read all at once.
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Note that JsonDocument needs to be disposed.
        using var configurationJson = new SmartConfigurationJsonDocument(JsonDocument.Parse(content));

        var validationErrors = validator.Validate(configurationJson, registration);

        if (validationErrors.Count > 0)
            return new SmartConfigurationDocumentResponse(SmartConfiguration.Empty, validationErrors);

        var configuration = new SmartConfiguration(issuer: configurationJson.Issuer,
            jwksUri: configurationJson.JwksUri,
            authorizationEndpoint: configurationJson.AuthorizationEndpoint,
            tokenEndpoint: configurationJson.TokenEndpoint,
            tokenEndpointAuthMethodsSupported: configurationJson.TokenEndpointAuthMethodsSupported,
            grantTypesSupported: configurationJson.GrantTypesSupported,
            registrationEndpoint: configurationJson.RegistrationEndpoint,
            scopesSupported: configurationJson.ScopesSupported,
            responseTypesSupported: configurationJson.ResponseTypesSupported,
            managementEndpoint: configurationJson.ManagementEndpoint,
            introspectionEndpoint: configurationJson.IntrospectionEndpoint,
            revocationEndpoint: configurationJson.RevocationEndpoint,
            codeChallengeMethodsSupported: configurationJson.CodeChallengeMethodsSupported,
            capabilities: configurationJson.Capabilities,
            associatedEndpoints: configurationJson.AssociatedEndpoints);

        var entryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetAbsoluteExpiration(options.Value.DiscoveryCacheDuration);

        smartConfigurationCache.Cache.Set(uri, configuration, entryOptions);
        
        return new SmartConfigurationDocumentResponse(configuration);
    }
}
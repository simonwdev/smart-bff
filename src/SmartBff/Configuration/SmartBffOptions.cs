using Microsoft.AspNetCore.Http;
using SmartBff.Extensions;

namespace SmartBff.Configuration;

public class SmartBffOptions
{
    public const string Key = "Bff";
    
    /// <summary>
    /// Base bath to host the Bff endpoints.
    /// Defaults to /smart-bff.
    /// </summary>
    public PathString BasePath { get; set; } = new(Constants.Endpoints.DefaultBase);
    /// <summary>
    /// Default login cookie duration.
    /// Defaults to 10 minutes.
    /// </summary>
    public TimeSpan LoginCookieDuration { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan SessionCleanupDuration { get; set; } = TimeSpan.FromMinutes(15);
    /// <summary>
    /// Percentage threshold for refreshing access tokens before their expiry
    /// to minimise clock-skew issues.
    /// Defaults to 80%.
    /// </summary>
    public double AccessTokenExpiryThresholdPercentage { get; set; } = 0.8;
    /// <summary>
    /// Header name for CSRF anti-forgery protection.
    /// Defaults to 'X-CSRF'.
    /// </summary>
    public string AntiforgeryHeaderName { get; set; } = "X-CSRF";
    /// <summary>
    /// Header value that must be used for CSRF anti-forgery protection.
    /// Defaults to '1'.
    /// </summary>
    public string AntiforgeryHeaderValue { get; set; } = "1";
    /// <summary>
    /// Number of Smart-on-FHIR discovery documents to be cached.
    /// Defaults to '1000'.
    /// </summary>
    public int DiscoveryCacheSize { get; set; } = 1000;
    /// <summary>
    /// Duration to retain Smart-on-FHIR discovery documents in the cache.
    /// Defaults to 24 hours.
    /// </summary>
    public TimeSpan DiscoveryCacheDuration { get; set; } = TimeSpan.FromHours(24);
    
    /// <summary>
    /// Determines if only a session identifier is sent
    /// to the client. This reduces the size of the cookies
    /// and increases security.
    /// </summary>
    public bool UseServerSideCookieStore { get; set; }
    
    /// <summary>
    /// Determines if the registration discriminator value can be used for launch.
    /// </summary>
    public bool AllowLaunchDiscriminator { get; set; }
    
    public IReadOnlyList<Registration> Registrations { get; set; } = [];
    
    public PathString LaunchPath => BasePath.Add(Constants.Endpoints.Launch);
    public PathString LoginCallbackPath => BasePath.Add(Constants.Endpoints.LoginCallback);
    public PathString SessionPath => BasePath.Add(Constants.Endpoints.Session);
    public PathString LogoutPath => BasePath.Add(Constants.Endpoints.Logout);

    /// <summary>
    /// Validates the options and raises an exception when invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Raised when invalid.</exception>
    public void EnsureValid()
    {
        if (BasePath == null || BasePath == "/")
            throw new InvalidOperationException("Base path must be provided and must not be the root path.");
        
        if (string.IsNullOrWhiteSpace(AntiforgeryHeaderName) || string.IsNullOrWhiteSpace(AntiforgeryHeaderValue))
            throw new InvalidOperationException("CSRF header details must be provided.");
        
        if (DiscoveryCacheSize < 0)
            throw new InvalidOperationException("Discovery cache size must be provided.");
        
        // Ensure registration identifiers are not used in multiple registrations.
        if (Registrations.Count != Registrations.Select(r => r.RegistrationId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count())
        {
            throw new InvalidOperationException("Registration identifiers must be unique.");
        }
        
        // Ensure issuer and discriminator pairs are not used in multiple registrations.
        if (Registrations.Count != Registrations.Select(r => $"{r.Issuer}:{r.Discriminator}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count())
        {
            throw new InvalidOperationException("Registration issuers and disriminators must be unique.");
        }
        
        foreach (var registration in Registrations)
        {
            if (string.IsNullOrWhiteSpace(registration.ClientId))
                throw new InvalidOperationException("ClientId is not valid.");
        
            if (string.IsNullOrWhiteSpace(registration.ClientSecret))
                throw new InvalidOperationException("ClientSecret is not valid.");
            
            if (string.IsNullOrWhiteSpace(registration.RegistrationId))
                throw new InvalidOperationException("RegistrationId is not valid.");

            if (!UrlHelper.IsAbsoluteUrlWithPathOnly(registration.Issuer, registration.Options.RequireHttps))
                throw new InvalidOperationException("Issuer is not a valid URL.");
            
            if (string.IsNullOrWhiteSpace(registration.MetadataAddress))
                registration.MetadataAddress = UrlHelper.Combine(registration.Issuer, Constants.Discovery.SmartDiscoveryEndpoint);

            if (!UrlHelper.IsAbsoluteUrlWithPathOnly(registration.MetadataAddress))
                throw new InvalidOperationException("MetadataAddress is not an absolute URI.");
            
            if (!UrlHelper.IsAbsoluteUrl(registration.LoginCallbackUrl, registration.Options.RequireHttps))
                throw new InvalidOperationException("LoginCallbackUrl is not valid.");   
        }
    }

    public Registration? GetRegistrationById(string registrationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationId);
        
        var registrations = Registrations
            .Where(a => a.RegistrationId.Equals(registrationId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (registrations.Length > 1)
            throw new InvalidOperationException("Multiple registrations exist for the registration id.");

        return registrations.FirstOrDefault();
    }
    
    public Registration? GetRegistrationByIssuerAndDiscriminator(string issuer, string discriminator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(discriminator);
        
        var registrations = Registrations
            .Where(a => a.Issuer.Equals(issuer, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(a.Discriminator)
                && a.Discriminator.Equals(discriminator, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (registrations.Length > 1)
            throw new InvalidOperationException("Multiple registrations exist for the registration id and issuer.");

        return registrations.FirstOrDefault();
    }
    
    public Registration? GetRegistrationByIssuer(string issuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        
        var registrations = Registrations
            .Where(a => a.Issuer.Equals(issuer, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (registrations.Length > 1)
            throw new InvalidOperationException("Multiple registrations exist for the issuer.");

        return registrations.FirstOrDefault();
    }
}
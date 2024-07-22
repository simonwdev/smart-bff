using IdentityModel;
using SmartBff.Configuration;
using SmartBff.Extensions;

namespace SmartBff.Features.Discovery;

public class SmartConfigurationValidator : ISmartConfigurationValidator
{
    public List<string> Validate(SmartConfigurationJsonDocument jsonDocument, Registration registration)
    {
        var result = new List<string>();
       
        if (registration.Options is { RequireIssuer: true, ValidateEndpoints: true } && !UrlHelper.IsAbsoluteUrl(jsonDocument.Issuer, requireSecure: registration.Options.RequireHttps))
            result.Add("Issuer is not a valid URL.");

        if (registration.Options.ValidateEndpoints)
        {
            if (string.IsNullOrWhiteSpace(jsonDocument.AuthorizationEndpoint))
                result.Add($"Endpoint 'AuthorizationEndpoint' is not specified but is mandatory.");
            
            if (string.IsNullOrWhiteSpace(jsonDocument.AuthorizationEndpoint))
                result.Add($"Endpoint 'AuthorizationEndpoint' is not specified but is mandatory.");
            
            foreach (var element in jsonDocument.JsonElement.EnumerateObject())
            {
                if (!element.Name.EndsWith("endpoint", StringComparison.OrdinalIgnoreCase) &&
                    !element.Name.Equals(OidcConstants.Discovery.JwksUri, StringComparison.OrdinalIgnoreCase)) 
                    continue;
                
                var endpoint = element.Value.ToString();
                    
                if (!string.IsNullOrWhiteSpace(endpoint) && !UrlHelper.IsAbsoluteUrl(endpoint, requireSecure: registration.Options.RequireHttps))
                    result.Add($"Endpoint '{element.Name}' is not a valid URL.");
            }
        }
        
        return result;
    }
}
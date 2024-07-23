using Microsoft.Extensions.Logging;

namespace SmartBff.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        Message = "Launch request successfully complete for '{issuer}'",
        Level = LogLevel.Information)]
    internal static partial void LaunchRequestSuccess(
        this ILogger logger,
        string issuer);
    
    [LoggerMessage(
        Message = "Login callback request successfully complete for '{path}'",
        Level = LogLevel.Information)]
    internal static partial void LoginCallbackRequestSuccess(
        this ILogger logger,
        string path);
    
    [LoggerMessage(
        Message = "Session request successfully complete for '{registrationId}'",
        Level = LogLevel.Information)]
    internal static partial void SessionRequestSuccess(
        this ILogger logger,
        string registrationId);
    
    [LoggerMessage(
        Message = "Logout request successfully complete for '{returnUrl}'",
        Level = LogLevel.Information)]
    internal static partial void LogoutRequestSuccess(
        this ILogger logger,
        string returnUrl);
    
    [LoggerMessage(
        Message = "Cache miss '{name}' '{key}'.",
        Level = LogLevel.Information)]
    internal static partial void CacheMiss(
        this ILogger logger,
        string name,
        string key);
    
    [LoggerMessage(
        Message = "Cache hit '{name}' '{key}'.",
        Level = LogLevel.Information)]
    internal static partial void CacheHit(
        this ILogger logger,
        string name,
        string key);
    
    [LoggerMessage(
        Message = "Anti-forgery header validation failed on path '{path}' for header '{header}'.",
        Level = LogLevel.Error)]
    internal static partial void AntiForgeryValidationFailed(
        this ILogger logger,
        string path,
        string header);
    
    [LoggerMessage(
        Message = "No parameter '{parameter}' supplied for request '{path}'.",
        Level = LogLevel.Error)]
    internal static partial void NoParameter(
        this ILogger logger,
        string path,
        string parameter);
    
    [LoggerMessage(
        Message = "Registration lookup failed for '{path}' with value '{value}'.",
        Level = LogLevel.Error)]
    internal static partial void RegistrationLookupFailed(
        this ILogger logger,
        string path,
        string value);
    
    [LoggerMessage(
        Message = "Smart Configuration is invalid '{errors}'.",
        Level = LogLevel.Error)]
    internal static partial void InvalidSmartConfiguration(
        this ILogger logger,
        string errors);
    
    [LoggerMessage(
        Message = "Token request failed '{error}:{errorDescription}'.",
        Level = LogLevel.Error)]
    internal static partial void TokenRequestFailed(
        this ILogger logger,
        string? error = "n/a",
        string? errorDescription = "n/a");
    
    [LoggerMessage(
        Message = "Authorisation code token request failed '{error}:{errorDescription}'.",
        Level = LogLevel.Error)]
    internal static partial void AuthorisationCodeTokenRequestFailed(
        this ILogger logger,
        string? error = "n/a",
        string? errorDescription = "n/a");
    
    [LoggerMessage(
        Message = "Authorisation callback returned an error '{error}:{errorDescription}'.",
        Level = LogLevel.Error)]
    internal static partial void AuthorisationCallbackFailed(
        this ILogger logger,
        string? error = "n/a",
        string? errorDescription = "n/a");
    
    [LoggerMessage(
        Message = "State parameter expected '{expected}' but got '{actual}'.",
        Level = LogLevel.Error)]
    internal static partial void InvalidStateParameter(
        this ILogger logger,
        string expected,
        string actual);
    
    [LoggerMessage(
        Message = "Return URL '{url}' is not valid.",
        Level = LogLevel.Error)]
    internal static partial void InvalidReturnUrl(
        this ILogger logger,
        string url);    
    
    [LoggerMessage(
        Message = "Cookie '{name}' was not found on request.",
        Level = LogLevel.Error)]
    internal static partial void NoCookie(
        this ILogger logger,
        string name);    
    
    [LoggerMessage(
        Message = "Token revocation for '{registrationId}' '{name}' failed '{error}'.",
        Level = LogLevel.Error)]
    internal static partial void TokenRevocationFailed(
        this ILogger logger,
        string registrationId,
        string name,
        string error);        
    
    [LoggerMessage(
        Message = "Token revocation for '{registrationId}' '{name}' complete.",
        Level = LogLevel.Information)]
    internal static partial void TokenRevocationComplete(
        this ILogger logger,
        string registrationId,
        string name);  
    
    [LoggerMessage(
        Message = "Session '{name}' exceeded max duration.",
        Level = LogLevel.Information)]
    internal static partial void SessionMaxDurationExceeded(
        this ILogger logger,
        string name);      
    
    [LoggerMessage(
        Message = "Session '{name}' has invalid state.",
        Level = LogLevel.Error)]
    internal static partial void SessionStateInvalid(
        this ILogger logger,
        string name);          
}
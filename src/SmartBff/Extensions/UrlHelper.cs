using System.Diagnostics.CodeAnalysis;

namespace SmartBff.Extensions;

public static class UrlHelper
{
    /// <summary>
    /// Combines two strings together with a URL separator.
    /// </summary>
    public static string Combine(string baseUrl, string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        if (string.IsNullOrWhiteSpace(relativeUrl))
            return baseUrl;

        baseUrl = baseUrl.TrimEnd('/');
        relativeUrl = relativeUrl.TrimStart('/');
                
        return $"{baseUrl}/{relativeUrl}";
    }
    
    /// <summary>
    /// Returns true if the returnUrl is valid and safe to redirect to.
    /// </summary>
    public static bool IsLocal(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return false;
        }

        switch (returnUrl[0])
        {
            // Allows "/" or "/foo" but not "//" or "/\".
            // url is exactly "/"
            case '/' when returnUrl.Length == 1:
                return true;
            // url doesn't start with "//" or "/\"
            case '/' when returnUrl[1] != '/' && returnUrl[1] != '\\':
                return !HasControlCharacter(returnUrl.AsSpan(1));
            case '/':
                return false;
        }

        return false;


        static bool HasControlCharacter(ReadOnlySpan<char> readOnlySpan)
        {
            // URLs may not contain ASCII control characters.
            foreach (var t in readOnlySpan)
            {
                if (char.IsControl(t))
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    public static bool TryCreateAbsoluteUri(string? url, [NotNullWhen(true)] out Uri? uri)
    {
        uri = null;
        
        return !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.IsWellFormedOriginalString();
    }

    /// <summary>
    /// Returns true if the URI is an HTTP or HTTPS URL.
    /// </summary>
    public static bool IsUrlScheme(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase) ||
            uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
    }
    
    /// <summary>
    /// Returns true if the URI is an HTTPS URL.
    /// </summary>
    public static bool IsSecureUrl(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        
        return uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
    }
    
    /// <summary>
    /// Returns true is the URI is an HTTPS URL.
    /// </summary>
    public static bool IsAbsoluteUrl(string? url, bool requireSecure = true)
    {
        return TryCreateAbsoluteUri(url, out var uri) && IsUrlScheme(uri) && (!requireSecure || IsSecureUrl(uri));
    }
    
    public static bool IsAbsoluteUrl(Uri uri, bool requireSecure = true)
    {
        return uri.IsAbsoluteUri && uri.IsWellFormedOriginalString() && IsUrlScheme(uri) && (!requireSecure || IsSecureUrl(uri));
    }
    
    public static bool IsAbsoluteUrlWithPathOnly(string url, bool requireSecure = true)
    {
        return TryCreateAbsoluteUri(url, out var uri)
            && IsAbsoluteUrlWithPathOnly(uri, requireSecure: requireSecure);
    }
    
    public static bool IsAbsoluteUrlWithPathOnly(Uri uri, bool requireSecure = true)
    {
        return uri.IsAbsoluteUri 
            && uri.IsWellFormedOriginalString() 
            && IsUrlScheme(uri) 
            && (!requireSecure || IsSecureUrl(uri))
            && string.IsNullOrEmpty(uri.Fragment) 
            && string.IsNullOrEmpty(uri.Query);
    }
}
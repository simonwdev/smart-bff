using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartBff.Extensions;

public static class ClaimExtensions
{
    /// <summary>
    /// Gets a <see cref="Claim"/> value from the <see cref="ClaimsPrincipal"/>
    /// or returns null if not found.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="type">The claim type to search for.</param>
    /// <returns>The claims value or null.</returns>
    /// <exception cref="InvalidOperationException">Raised when multiple claims exist with the same type.</exception>
    public static string? GetOptionalClaimValue(this ClaimsPrincipal principal, string type)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var claims = principal.FindAll(type).ToList();
        if (claims.Count > 1)
            throw new InvalidOperationException($"Multiple claims exist for '{type}'");

        return claims.FirstOrDefault()?.Value;
    }
    
    /// <summary>
    /// Gets a <see cref="Claim"/> value from the <see cref="ClaimsPrincipal"/>
    /// or raises an exception if not found.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="type">The claim type to search for.</param>
    /// <returns>The claim.</returns>
    /// <exception cref="InvalidOperationException">Raised when multiple claims exist or the claim is not found.</exception>
    public static Claim GetRequiredClaim(this ClaimsPrincipal principal, string type)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(type);

        var claims = principal.FindAll(type).ToList();

        return claims.Count switch
        {
            > 1 => throw new InvalidOperationException($"Multiple claims exist for '{type}'"),
            0 => throw new InvalidOperationException($"No claims exist for '{type}'"),
            _ => claims.First()
        };
    }

    /// <summary>
    /// Gets a <see cref="Claim"/> value from the <see cref="ClaimsPrincipal"/>
    /// or null if not found.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="type">The claim type to search for.</param>
    /// <returns>The claim.</returns>
    /// <exception cref="InvalidOperationException">Raised when multiple claims exist.</exception>
    public static string GetRequiredClaimValue(this ClaimsPrincipal principal, string type)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.GetRequiredClaim(type).Value;
    }
    
    /// <summary>
    /// Adds a claim to the <see cref="ClaimsIdentity"/>.
    /// </summary>
    /// <param name="identity">The identity the claim will be added to.</param>
    /// <param name="type">The claim type.</param>
    /// <param name="value">The value of the claim.</param>
    public static void AddClaim(this ClaimsIdentity identity, string type, string value)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(value);

        identity.AddClaim(new Claim(type, value));
    }

    /// <summary>
    /// Gets a <see cref="Claim"/> value from the <see cref="JwtSecurityToken"/>
    /// or null if not found.
    /// </summary>
    /// <param name="token">The token to search for the claim.</param>
    /// <param name="type">The claim type to search for.</param>
    /// <returns>The claim.</returns>
    /// <exception cref="InvalidOperationException">Raised when multiple claims exist or the claim is not found.</exception>
    public static string GetRequiredClaimValue(this JwtSecurityToken token, string type)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(type);

        var claims = token.Claims.Where(a => a.Type == type).ToList();

        return claims.Count switch
        {
            > 1 => throw new InvalidOperationException($"Multiple claims exist for '{type}'"),
            0 => throw new InvalidOperationException($"No claims exist for '{type}'"),
            _ => claims.First().Value
        };
    }
}
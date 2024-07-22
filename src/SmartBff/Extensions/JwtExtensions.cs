using System.IdentityModel.Tokens.Jwt;

namespace SmartBff.Extensions;

public static class JwtExtensions
{
    private static readonly JwtSecurityTokenHandler DefaultHandler = new() { MapInboundClaims = false };

    /// <summary>
    /// Converts a string into an instance of <see cref="T:System.IdentityModel.Tokens.Jwt.JwtSecurityToken" />.
    /// </summary>
    /// <param name="token">A 'JSON Web Token' (JWT) in JWS or JWE Compact Serialization Format.</param>
    /// <returns>A <see cref="T:System.IdentityModel.Tokens.Jwt.JwtSecurityToken" /></returns>
    public static JwtSecurityToken ReadToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return DefaultHandler.ReadJwtToken(token);
    }
    
    /// <summary>
    /// Determines if a <see cref="JwtSecurityToken"/> has expired or is close to expiry.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    /// <param name="percentage">The percentage to determine if the token is close to expiry.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Raised if the percentage is not valid.</exception>
    public static bool IsExpiredOrExpiring(this JwtSecurityToken token, DateTime nowUtc, double percentage = 1)
    {
        ArgumentNullException.ThrowIfNull(token);
        
        if (percentage is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        
        var difference = token.ValidTo - nowUtc;
        var percentOfTimeAsTicks = (long)(percentage * difference.Ticks);
        var percentageUtc = nowUtc + new TimeSpan(percentOfTimeAsTicks);

        return nowUtc > percentageUtc;
    }
}
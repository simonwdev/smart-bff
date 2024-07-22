using IdentityModel.Client;

namespace SmartBff.Configuration;

public class RegistrationOptions
{
    public bool RequireHttps { get; set; } = true;
    public bool RequireIssuer { get; set; } = true;
    public bool ValidateEndpoints { get; set; } = true;
    public TimeSpan RefreshTokenDuration { get; set; } = TimeSpan.FromMinutes(60);
    
    public bool RevokeOnLogout { get; set; }
    
    public BasicAuthenticationHeaderStyle RevocationBasicAuthenticationHeaderStyle { get; set; }  
}
using IdentityModel.Client;

namespace SmartBff.Configuration;

public class RegistrationOptions
{
    public bool RequireHttps { get; set; } = true;
    public bool RequireIssuer { get; set; } = true;
    public bool ValidateEndpoints { get; set; } = true;
    public TimeSpan SessionSlidingDuration { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan SessionMaxDuration { get; set; } = TimeSpan.FromHours(2);
    
    public bool RevokeOnLogout { get; set; }
    
    public BasicAuthenticationHeaderStyle RevocationBasicAuthenticationHeaderStyle { get; set; }  
}
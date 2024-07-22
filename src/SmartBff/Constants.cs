namespace SmartBff;

public static class Constants
{
    public static class CustomClaims
    {
        public const string RegistrationId = "sb_registration_id";
        public const string ReturnUrl = "sb_return_url";
        public const string LoginCallback = "sb_login_callback";
        public const string PkceCodeVerifier = "sb_claim_verifier";
        public const string State = "sb_state";
        public const string AccessToken = "sb_access_token";
        public const string RefreshToken = "sb_refresh_token";
        public const string IdentityToken = "sb_id_token";
    }
    
    public static class Endpoints
    {
        public const string DefaultBase = "/smart-bff";
        public const string Launch = "/launch";
        public const string LoginCallback = "/callback/login/{name}";
        public const string Session = "/session";
        public const string Logout = "/logout";
    }
    
    public static class QueryParameters
    {
        public const string DiscriminatorQueryParameter = "discriminator";
        public const string ReturnUrlQueryParameter = "returnUrl";
        public const string IssuerQueryParameter = "iss";
        public const string LaunchQueryParameter = "launch";
    }
    
    public static class Discovery
    {
        public const string AssociatedEndpoints = "associate_endpoints";
        public const string AssociatedEndpointsUrl = "url";
        public const string ManagementEndpoint = "management_endpoint";
        public const string Capabilities = "capabilities";
        public const string SmartDiscoveryEndpoint = "/.well-known/smart-configuration";
    }
    
    public static class AuthenticationProperties
    {
        public const string RegistrationId = "SmartBff.RegistrationId";
        public const string RefreshTokenDuration = "SmartBff.RefreshTokenDuration";
    }
    
    public static class AuthenticationSchemes
    {
        public const string Login = "SmartBff.Login";
        public const string Session = "SmartBff.Session";
    }
    
    public static class AuthorizationPolicies
    {
        public const string Session = "SmartBff.Session";
    }
}
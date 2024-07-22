namespace SmartBff.Endpoints.Session;

public class GetSessionResponse(string name, string accessToken, string? idToken)
{
    public string Name { get; set; } = name;
    public string AccessToken { get; set; } = accessToken;
    public string? IdToken { get; set; } = idToken;
}
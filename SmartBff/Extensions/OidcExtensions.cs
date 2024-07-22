using System.Net.Http.Headers;
using IdentityModel.Client;
using SmartBff.Configuration;

namespace SmartBff.Extensions;

public static class OidcExtensions
{
    public static async Task<RevocationResponse[]> RevokeTokensAsync(this HttpClient httpClient, 
        Registration registration, 
        Uri endpoint, 
        RevocationToken[] tokens)
    {
        return await Task.WhenAll(tokens.Select(RevokeTokenAsync));
        
        async Task<RevocationResponse> RevokeTokenAsync(RevocationToken token)
        {
            using var request = new TokenRevocationRequest();
            request.RequestUri = endpoint;
            request.ClientId = registration.ClientId;
            request.ClientSecret = registration.ClientSecret;
            request.Token = token.TokenValue;
            request.TokenTypeHint = token.TokenType;
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.AuthorizationHeaderStyle = registration.Options.RevocationBasicAuthenticationHeaderStyle;
            
            var response = await httpClient.RevokeTokenAsync(request);

            return new RevocationResponse(token, response);
        }
    }
}

public record RevocationToken(string TokenValue, string TokenType);
public record RevocationResponse(RevocationToken RevocationToken, TokenRevocationResponse TokenRevocationesponse);
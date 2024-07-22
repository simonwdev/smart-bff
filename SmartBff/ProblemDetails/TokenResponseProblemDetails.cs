using System.Net;
using IdentityModel.Client;

namespace SmartBff.ProblemDetails;

public class TokenResponseProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public TokenResponseProblemDetails(TokenResponse tokenResponse)
    {
        Detail = "Token request rejected by authorization server.";
        Status = (int)HttpStatusCode.BadRequest;
        Extensions = new Dictionary<string, object?>()
        {
            { "error", tokenResponse.Error },
            { "error_description", tokenResponse.ErrorDescription },
            { "errorType", tokenResponse.ErrorType },
        };
    }
}
using System.Net;

namespace SmartBff.ProblemDetails;


public class AuthorizeCallbackProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public AuthorizeCallbackProblemDetails(string error, string? errorDescription)
    {
        Detail = "Authorization request rejected by authorization server.";
        Status = (int)HttpStatusCode.BadRequest;
        Extensions = new Dictionary<string, object?>()
        {
            { "error", error },
            { "error_description", errorDescription }
        };
    }

    public static AuthorizeCallbackProblemDetails New(string error, string? errorDescription)
    {
        return new AuthorizeCallbackProblemDetails(error, errorDescription);
    }
}
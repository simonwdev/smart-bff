using System.Net;
using SmartBff.Features.Discovery;

namespace SmartBff.ProblemDetails;

public class SmartDocumentProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public SmartDocumentProblemDetails(SmartConfigurationDocumentResponse documentResponse)
    {
        Detail = "The Smart Configuration metadata is not valid.";
        Status = (int)HttpStatusCode.UnprocessableContent;
        Extensions = new Dictionary<string, object?>() { { "ValidationErrors", documentResponse.ValidationErrors } };
    }
}
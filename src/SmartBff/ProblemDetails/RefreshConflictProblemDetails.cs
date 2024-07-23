using System.Net;
using IdentityModel.Client;

namespace SmartBff.ProblemDetails;

public class RefreshConflictProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public RefreshConflictProblemDetails()
    {
        Detail = "Duplicate calls detected.";
        Status = (int)HttpStatusCode.Conflict;
    }
}
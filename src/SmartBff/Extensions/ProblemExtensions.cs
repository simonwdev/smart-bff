using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SmartBff.Extensions;

public static class ProblemExtensions
{
    /// <summary>
    /// Write problem details to the current response context,
    /// attempting to use <see cref="IProblemDetailsService"/> but
    /// falling back to plain text if required. 
    /// </summary>
    public static async Task WriteProblemAsync(this HttpResponse httpResponse, HttpStatusCode statusCode, string? detail = null)
    {
        await WriteProblemAsync(httpResponse, new Microsoft.AspNetCore.Mvc.ProblemDetails() { Status = (int)statusCode, Detail = detail });
    }
    
    /// <summary>
    /// Write a <see cref="ProblemDetails"/> to the current response context,
    /// attempting to use <see cref="IProblemDetailsService"/> but
    /// falling back to plain text if required. 
    /// </summary>
    public static async Task WriteProblemAsync(this HttpResponse httpResponse, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        var httpContext = httpResponse.HttpContext;
        var statusCode = problemDetails.Status ?? 500;
        var detail = problemDetails.Detail;
        
        httpContext.Response.StatusCode = statusCode;
        
        var problemDetailsService = httpContext.RequestServices.GetService<IProblemDetailsService>();
        if (problemDetailsService != null)
        {
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
            });
        }
        else
        {
            // Determine if the status code pages middleware has been enabled for this request.
            // If enabled we can skip processing.
            var feature = httpContext.Features.Get<IStatusCodePagesFeature>();
            if (feature is { Enabled: true })
                return;
            
            httpContext.Response.ContentType = "text/plain;charset=UTF-8";
            
            await httpContext.Response.WriteAsync($"{statusCode}");
            
            if (!string.IsNullOrWhiteSpace(detail))
                await httpContext.Response.WriteAsync($": {detail}");
        }
    }
}
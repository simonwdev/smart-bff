using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartBff.Configuration;
using SmartBff.Extensions;

namespace SmartBff.Middleware.Antiforgery;

/// <summary>
/// Middleware to provide anti-forgery protection via a static header and 302 to 401 conversion
/// Must run *before* the authorization middleware
/// </summary>
public class AntiforgeryHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AntiforgeryHeaderMiddleware> _logger;
    private readonly IOptions<SmartBffOptions> _options;

    /// <summary>
    /// ctor
    /// </summary>
    public AntiforgeryHeaderMiddleware(
        RequestDelegate next,
        ILogger<AntiforgeryHeaderMiddleware> logger,
        IOptions<SmartBffOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Request processing
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);

            return;
        }

        var requiresProtection = endpoint.Metadata.GetMetadata<IAntiforgeryHeader>() != null;
        if (requiresProtection)
        {
            var isSkipped = endpoint.Metadata.GetMetadata<ISkipAntiforgeryHeader>() != null;
            if (!isSkipped)
            {
                var antiForgeryHeader = context.Request.Headers[_options.Value.AntiforgeryHeaderName].FirstOrDefault();
                if (antiForgeryHeader != _options.Value.AntiforgeryHeaderValue)
                {
                    _logger.AntiForgeryValidationFailed(context.Request.Path, _options.Value.AntiforgeryHeaderName);

                    await context.Response.WriteProblemAsync(HttpStatusCode.Unauthorized, "CSRF protection failure.");

                    return;
                }
            }
        }

        await _next(context);
    }
}
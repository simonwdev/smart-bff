using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartBff.Configuration;
using SmartBff.Features.Discovery;

namespace SmartBff.Endpoints;

/// <summary>
/// Service provider for bff route minimal api.
/// </summary>
public class EndpointServices(
    [FromServices] IHttpClientFactory httpClientFactory,
    [FromServices] ISmartConfigurationService smartConfigurationService,
    [FromServices] IOptions<SmartBffOptions> options,
    [FromServices] ILogger<EndpointServices> logger)
{
    public IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;
    public ISmartConfigurationService SmartConfigurationService { get; } = smartConfigurationService;
    public IOptions<SmartBffOptions> Options { get; } = options;
    public ILogger<EndpointServices> Logger { get; } = logger;
}
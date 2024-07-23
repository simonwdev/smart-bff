using Microsoft.Extensions.DependencyInjection;

namespace SmartBff.Configuration;

public sealed class SmartBffBuilder : ISmartBffBuilder
{
    /// <summary>
    /// Creates a new configuration object linked to a <see cref="IServiceCollection"/>.
    /// </summary>
    public SmartBffBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    public IServiceCollection Services { get; }
}
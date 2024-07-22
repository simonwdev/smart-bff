using Microsoft.Extensions.Caching.Memory;

namespace SmartBff.Features.Discovery;

public interface ISmartConfigurationCache
{
    IMemoryCache Cache { get; }
}
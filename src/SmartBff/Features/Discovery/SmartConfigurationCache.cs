using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SmartBff.Configuration;

namespace SmartBff.Features.Discovery;

public class SmartConfigurationCache(IOptions<SmartBffOptions> options) : ISmartConfigurationCache
{
    public IMemoryCache Cache { get; } = new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = options.Value.DiscoveryCacheSize
        });
}
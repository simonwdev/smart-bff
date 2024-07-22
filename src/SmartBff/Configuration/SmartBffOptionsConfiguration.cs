using Microsoft.Extensions.Options;

namespace SmartBff.Configuration;

public sealed class SmartBffOptionsConfiguration : IPostConfigureOptions<SmartBffOptions>
{
    public void PostConfigure(string? name, SmartBffOptions options)
    {
        options.EnsureValid();
    }
}
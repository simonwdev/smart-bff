using Microsoft.Extensions.DependencyInjection;

namespace SmartBff.Configuration;

public interface ISmartBffBuilder
{
    IServiceCollection Services { get; }
}
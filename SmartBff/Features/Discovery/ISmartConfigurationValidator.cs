using SmartBff.Configuration;

namespace SmartBff.Features.Discovery;

public interface ISmartConfigurationValidator
{
    List<string> Validate(SmartConfigurationJsonDocument jsonDocument, Registration registration);
}
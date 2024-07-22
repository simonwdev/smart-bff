using SmartBff.Configuration;

namespace SmartBff.Features.Discovery;

public interface ISmartConfigurationService
{
    Task<SmartConfigurationDocumentResponse> GetDiscoveryDocumentAsync(Registration registration, CancellationToken cancellationToken = default);
}
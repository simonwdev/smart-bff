namespace SmartBff.Features.Discovery;

public class SmartConfigurationDocumentResponse(SmartConfiguration configuration, List<string>? validatorErrors = null)
{
    public bool IsValid => ValidationErrors.Count == 0;
    public SmartConfiguration Configuration { get; } = configuration;
    public List<string> ValidationErrors { get; } = validatorErrors ?? [];
}
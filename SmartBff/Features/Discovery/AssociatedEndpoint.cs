namespace SmartBff.Features.Discovery;

public class AssociatedEndpoint(
    string? url,
    List<string>? capabilities)
{
    public string? Url { get; } = url;
    public IReadOnlyList<string>? Capabilities { get; } = capabilities;
}
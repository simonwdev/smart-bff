namespace SmartBff.Middleware.Antiforgery;

/// <summary>
/// Marks an endpoint as not intended for Ajax requests which can ignore the CSRF header.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipAntiforgeryHeaderAttribute : Attribute, ISkipAntiforgeryHeader
{
    public static readonly SkipAntiforgeryHeaderAttribute Instance = new();
}
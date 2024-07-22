namespace SmartBff.Middleware.Antiforgery;

/// <summary>
/// Marks an endpoint as intended for Ajax requests which requires CSRF protection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AntiforgeryHeaderAttribute : Attribute, IAntiforgeryHeader
{
    public static readonly AntiforgeryHeaderAttribute Instance = new();
}
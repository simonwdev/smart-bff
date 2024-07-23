using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SmartBff.Endpoints.Launch;
using SmartBff.Endpoints.LoginCallback;
using SmartBff.Endpoints.Logout;
using SmartBff.Endpoints.Session;
using SmartBff.Features.Discovery;
using SmartBff.Middleware.Antiforgery;
using SmartBff.Middleware.CookieEvents;

namespace SmartBff.Configuration;

public static class SmartBffExtensions
{
    /// <summary>
    /// Registers the SmartBff services.
    /// </summary>
    public static ISmartBffBuilder AddSmartBff(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<SmartBffOptions>()
            .BindConfiguration(SmartBffOptions.Key)
            .PostConfigure(a => a.EnsureValid());
        
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Singleton<IPostConfigureOptions<SmartBffOptions>, SmartBffOptionsConfiguration>());
        
        serviceCollection.AddMemoryCache();
        
        serviceCollection.TryAddSingleton<ISmartConfigurationCache, SmartConfigurationCache>();

        serviceCollection.TryAddTransient<ISmartConfigurationService, SmartConfigurationService>();
        serviceCollection.TryAddTransient<ISmartConfigurationValidator, SmartConfigurationValidator>();
        serviceCollection.TryAddTransient<SessionCookieAuthenticationEvents>();
        serviceCollection.TryAddTransient<SinglePageCookieAuthenticationEvents>();
        
        // Update the login cookie options.
        serviceCollection.AddOptions<CookieAuthenticationOptions>(Constants.AuthenticationSchemes.Login)
            .PostConfigure<ISmartBffTicketStore, IOptions<SmartBffOptions>>((cookieOptions, ticketStore, bffOptions) => {
                cookieOptions.SessionStore = bffOptions.Value.UseServerSideCookieStore ? ticketStore : null;
                cookieOptions.Cookie.MaxAge = bffOptions.Value.LoginCookieDuration;
                cookieOptions.ExpireTimeSpan = bffOptions.Value.LoginCookieDuration;
            });
        
        // Update the session cookie options.
        serviceCollection.AddOptions<CookieAuthenticationOptions>(Constants.AuthenticationSchemes.Session)
            .PostConfigure<ISmartBffTicketStore, IOptions<SmartBffOptions>>((cookieOptions, ticketStore, bffOptions) => {
                cookieOptions.SessionStore = bffOptions.Value.UseServerSideCookieStore ? ticketStore : null;
            });
        
        return new SmartBffBuilder(serviceCollection);
    }

    public static ISmartBffBuilder PersistSessionsToDistributedCache(this ISmartBffBuilder builder)
    {
        builder.Services.TryAddSingleton<ISmartBffTicketStore, DistributedCacheTicketStore>();

        return builder;
    }
    
    /// <summary>
    /// Adds the SmartBff cookie authentication to AuthenticationBuilder.
    /// </summary>
    public static AuthenticationBuilder AddSmartBffSchemes(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        // Cookies MUST be http-only, encrypted, and secure.
        // Cookies MUST be 'lax' to enable cross-site" top-level navigations.
        
        builder.AddCookie(Constants.AuthenticationSchemes.Login, o =>
            {
                // Short-lived cookie for persisting authorisation flow details.
                o.Cookie.Name = Constants.AuthenticationSchemes.Login;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.IsEssential = true;
                o.Cookie.MaxAge = null; // Updated in PostConfigure.
                o.ExpireTimeSpan = default; // Updated in PostConfigure. 
                o.SlidingExpiration = false;
                o.EventsType = typeof(SinglePageCookieAuthenticationEvents);
            })
            .AddCookie(Constants.AuthenticationSchemes.Session, o =>
            {
                // Long-lived cookie for persisting access token, refresh token and identity token.
                o.Cookie.Name = Constants.AuthenticationSchemes.Session;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.IsEssential = true;
                o.Cookie.MaxAge = null; // Updated in SessionCookieAuthenticationEvents.
                o.ExpireTimeSpan = default; // Updated in SessionCookieAuthenticationEvents. 
                o.SlidingExpiration = true;
                o.EventsType = typeof(SessionCookieAuthenticationEvents);
            });

        return builder;
    }

    /// <summary>
    /// Adds the SmartBff session policy.
    /// </summary>
    public static AuthorizationBuilder AddSmartBffPolicy(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(Constants.AuthorizationPolicies.Session, b =>
        {
            b.AddAuthenticationSchemes(Constants.AuthenticationSchemes.Session);
            b.RequireAuthenticatedUser();
        });

        return builder;
    }

    /// <summary>
    /// Adds the SmartBff session policy to the endpoint.
    /// This requires that the request have a valid session cookie
    /// which means the user is logged into a registered Smart-on-FHIR
    /// authorisation server. 
    /// </summary>
    public static TBuilder RequireSmartBffAuthorization<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.RequireAuthorization(Constants.AuthorizationPolicies.Session);
    }

    /// <summary>
    /// Marks an endpoint as intended for Ajax requests which requires CSRF protection.
    /// </summary>
    public static IEndpointConventionBuilder WithAntiforgeryHeaderValidation(this IEndpointConventionBuilder builder)
    {
        return builder.WithMetadata(AntiforgeryHeaderAttribute.Instance);
    }

    /// <summary>
    /// Marks an endpoint as not intended for Ajax requests which can ignore the CSRF header.
    /// </summary>
    public static IEndpointConventionBuilder WithSkipAntiforgeryHeaderValidation(this IEndpointConventionBuilder builder)
    {
        return builder.WithMetadata(SkipAntiforgeryHeaderAttribute.Instance);
    }

    /// <summary>
    /// Adds middleware to perform XSRF/CSRF header validation.
    /// </summary>
    public static IApplicationBuilder UseCsrfHeaderValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AntiforgeryHeaderMiddleware>();
    }
    
    /// <summary>
    /// Adds the bff management endpoints.
    /// </summary>
    public static void MapBffEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<SmartBffOptions>>().Value;
        
        endpoints.MapGet(options.LaunchPath.Value!, LaunchApi.LaunchAsync)
            .WithSkipAntiforgeryHeaderValidation();
        
        endpoints.MapGet(options.LoginCallbackPath.Value!, LoginCallbackApi.LoginCallbackAsync)
            .WithSkipAntiforgeryHeaderValidation();
        
        endpoints.MapGet(options.SessionPath.Value!, GetSessionApi.GetSessionAsync)
            .WithAntiforgeryHeaderValidation();
        
        endpoints.MapGet(options.LogoutPath.Value!, LogoutApi.LogoutAsync)
            .WithSkipAntiforgeryHeaderValidation();
    }
}
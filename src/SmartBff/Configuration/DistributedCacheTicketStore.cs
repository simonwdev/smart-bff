using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

namespace SmartBff.Configuration;

/// <summary>
/// A <see cref="ITicketStore"/> implementation that backs onto
/// <see cref="IDistributedCache"/> using <see cref="IDataProtector"/> for
/// encryption at rest.
/// </summary>
public class DistributedCacheTicketStore(
    IDistributedCache cache,
    IDataProtectionProvider dataProtectionProvider) : ISmartBffTicketStore
{
    private const string KeyPrefix = "CookieTicketStore-";
    
    private readonly TicketSerializer _ticketSerializer = TicketSerializer.Default;
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("SmartBff.DistributedCacheTicketStore.v1");

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        
        var key = $"{KeyPrefix}{Guid.NewGuid():N}";
        
        await RenewAsync(key, ticket);
        
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        var options = new DistributedCacheEntryOptions();

        var expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc is not null)
            options.SetAbsoluteExpiration(expiresUtc.Value);
        
        if (ticket.Properties.AllowRefresh ?? false)
            options.SetSlidingExpiration(TimeSpan.FromMinutes(60));
        
        var ticketBytes = _ticketSerializer.Serialize(ticket);
        var encryptedBytes = _protector.Protect(ticketBytes);
        
        return cache.SetAsync(key, encryptedBytes, options);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        var encryptedBytes = await cache.GetAsync(key);

        if (encryptedBytes is null)
            return null;
        
        var ticketBytes = _protector.Unprotect(encryptedBytes);

        return _ticketSerializer.Deserialize(ticketBytes);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
}
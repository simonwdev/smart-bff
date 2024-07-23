using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartBff.Configuration;
using SmartBff.Extensions;

namespace SmartBff.EntityFrameworkCore;

public interface ITicketStoreCache
{
    IMemoryCache Cache { get; }
}

public class TicketStoreCache(IOptions<SmartBffOptions> options) : ITicketStoreCache
{
    public IMemoryCache Cache { get; } = new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = 1000
        });
}

/// <summary>
/// A <see cref="ITicketStore"/> implementation that backs onto a
/// <see cref="DbContext"/> using <see cref="IDataProtector"/> for
/// encryption at rest.
/// </summary>
public class EntityFrameworkCoreTicketStore<TContext>(
    IServiceProvider serviceProvider,
    IOptions<SmartBffOptions> options,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<EntityFrameworkCoreTicketStore<TContext>> logger) : ISmartBffTicketStore where TContext : DbContext, ISmartBffSessionContext
{
    private readonly TicketSerializer _ticketSerializer = TicketSerializer.Default;
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("SmartBff.EntityFrameworkCoreTicketStore.v1");
    private readonly object _expirationScanMutex = new();
    private readonly TimeSpan _expiredItemsDeletionInterval = options.Value.SessionCleanupDuration;
    private DateTimeOffset _lastExpirationScan;
    
    private byte[] SerializeToBytes(AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        
        var ticketBytes = _ticketSerializer.Serialize(ticket);
        var encryptedBytes = _protector.Protect(ticketBytes);

        return encryptedBytes;
    }
    private AuthenticationTicket DeserializeFromBytes(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        var ticketBytes = _protector.Unprotect(source);
        return _ticketSerializer.Deserialize(ticketBytes) ?? throw new InvalidOperationException("Failed to deserialize session.");
    }
    
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        var session = new SmartBffSession
        {
            Id = Guid.NewGuid(),
            Type = ticket.Principal.Identity?.AuthenticationType ?? "n/a",
            RegistrationId = ticket.Principal.GetRequiredClaimValue(Constants.CustomClaims.RegistrationId),
            Name = ticket.Principal.GetOptionalClaimValue(JwtClaimTypes.Name),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = ticket.Properties.ExpiresUtc?.DateTime,
            ConcurrencyToken = Guid.NewGuid(),
            Payload = SerializeToBytes(ticket)
        };
        
        dbContext.SmartBffSessions.Add(session);

        await dbContext.SaveChangesAsync();
        
        DeleteExpiredItems();
        
        return session.Id.ToString();
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        if (!Guid.TryParse(key, out var id))
            throw new InvalidOperationException("Key is not a valid guid.");
        
        var session = await dbContext.SmartBffSessions.FindAsync(id);
        if (session is not null)
        {
            session.Payload = SerializeToBytes(ticket);
            session.ExpiresOn = ticket.Properties.ExpiresUtc?.DateTime;
            session.ConcurrencyToken = Guid.NewGuid();
            
            await dbContext.SaveChangesAsync();
        }
        
        DeleteExpiredItems();
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        if (!Guid.TryParse(key, out var id))
            throw new InvalidOperationException("Key is not a valid guid.");
            
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        
        var session = await dbContext.SmartBffSessions.FindAsync(id);

        var ticket = session is null ? null : DeserializeFromBytes(session.Payload);
        
        DeleteExpiredItems();

        return ticket;
    }

    public async Task RemoveAsync(string key)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        if (!Guid.TryParse(key, out var id))
            throw new InvalidOperationException("Key is not a valid guid.");

        await dbContext.SmartBffSessions.Where(s => s.Id == id).ExecuteDeleteAsync();

        DeleteExpiredItems();
    }
    
    private void DeleteExpiredItems()
    {
        lock (_expirationScanMutex)
        {
            var utcNow = DateTime.UtcNow;
            
            if ((utcNow - _lastExpirationScan) <= _expiredItemsDeletionInterval) 
                return;
            
            _lastExpirationScan = utcNow;
            
            Task.Run(async () =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                await using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                
                await dbContext.SmartBffSessions.Where(s => utcNow > s.ExpiresOn).ExecuteDeleteAsync();
            });
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartBff.Configuration;

namespace SmartBff.EntityFrameworkCore;

public static class SmartBffExtensions
{
    public static ISmartBffBuilder PersistSessionsToDbContext<TDbContext>(this ISmartBffBuilder builder)
        where TDbContext : DbContext, ISmartBffSessionContext
    {
        builder.Services.TryAddSingleton<ITicketStoreCache, TicketStoreCache>();
        builder.Services.TryAddSingleton<ISmartBffTicketStore, EntityFrameworkCoreTicketStore<TDbContext>>();

        return builder;
    }
}
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartAppJs;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    // This maps to the table that stores keys.
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}

public class ApplicationDbContextWorker(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
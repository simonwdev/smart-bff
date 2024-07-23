using SmartAppJs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SmartBff.Configuration;
using SmartBff.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient();

builder.Services.AddSmartBff()
    //.PersistSessionsToDistributedCache()
    .PersistSessionsToDbContext<ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddSmartBffSchemes(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddSmartBffPolicy();

// builder.Services.AddSqliteCache(options => {
//     options.CachePath = Path.Combine(Path.GetTempPath(), "smartapp-cookies.sqlite3");
// });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine("C:\\data", "smartapp-db.sqlite3")}");
    options.EnableSensitiveDataLogging();
});
builder.Services.AddHostedService<ApplicationDbContextWorker>();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();
    // NOTE: Production data protection should use KeyVault etc to encrypt at rest.
    //.ProtectKeysWithAzureKeyVault("<keyIdentifier>", "<clientId>", "<clientSecret>");


// TODO
// - Lock on refresh token.
// - Lock on refresh token.

var app = builder.Build();
app.UseExceptionHandler(a =>
{
    a.Run(async c =>
    {
        var exceptionHandlerFeature = c.Features.Get<IExceptionHandlerFeature>();
        var exceptionType = exceptionHandlerFeature?.Error;
    });
});
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseCsrfHeaderValidation();
app.UseAuthentication();
app.MapBffEndpoints();

app.MapGet("/api/test", Endpoints.GetExternalService)
    .RequireSmartBffAuthorization()
    .WithAntiforgeryHeaderValidation();

// Proxy

app.Run();
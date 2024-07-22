using System.Security.Claims;
using SmartAppJs;
using IdentityModel;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NeoSmart.Caching.Sqlite.AspNetCore;
using SmartBff.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient();
builder.Services.AddSmartBff();
builder.Services.AddAuthentication()
    .AddSmartBffSchemes(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddSmartBffPolicy();

builder.Services.AddSqliteCache(options => {
    options.CachePath = Path.Combine(Path.GetTempPath(), "smartapp-cookies.sqlite3");
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "smartapp-db.sqlite3")}");
    options.EnableSensitiveDataLogging();
});
builder.Services.AddHostedService<ApplicationDbContextWorker>();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();
    // NOTE: Production data protection should use KeyVault etc to encrypt at rest.
    //.ProtectKeysWithAzureKeyVault("<keyIdentifier>", "<clientId>", "<clientSecret>");

var app = builder.Build();
app.UseExceptionHandler();
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

using MarketData.Application.HostedServices;
using MarketData.Application.Interfaces;
using MarketData.Domain.Interfaces;
using MarketData.Infrastructure.Configuration;
using MarketData.Infrastructure.Data;
using MarketData.Infrastructure.FintachartsApi;
using MarketData.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.Configure<FintachartsSettings>(builder.Configuration.GetSection("Finecharts"));

builder.Services.AddHttpClient<IFintachartsRestClient, FintachartsRestClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<FintachartsSettings>>().Value;
    client.BaseAddress = new Uri(settings.RestApiBaseUrl);
});
builder.Services.AddTransient<IFintachartsWsClient, FintachartsWsClient>();

builder.Services.AddHostedService<MarketDataBackgroundService>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MarketDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration error");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();

app.UseHttpsRedirection();

app.Run();


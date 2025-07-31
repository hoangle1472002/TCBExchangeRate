using TCBExchangeRate.Persistence;
using Microsoft.EntityFrameworkCore;
using TCBExchangeRate.Infrastructure.Interfaces;
using TCBExchangeRate.Application.Interfaces;
using TCBExchangeRate.Persistence.Services;
using TCBExchangeRate.Infrastructure.Services;
using TCBExchangeRate.Persistence.Data;
using TCBExchangeRate.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddTransient<CurrencySeeder>();

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<CurrencySeeder>();
        await seeder.SeedAsync();

        var service = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
        var totalSaved = await service.ImportExchangeRatesFromPastMonthAsync();
        logger.LogInformation("Done: {TotalSaved} exchange rates saved.", totalSaved);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing or importing exchange rates.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();

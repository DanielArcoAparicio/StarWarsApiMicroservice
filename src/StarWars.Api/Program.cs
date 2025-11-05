using System.Reflection;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using StarWars.Api.Middleware;
using StarWars.Application.Interfaces;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.External;
using StarWars.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Star Wars API",
        Version = "v1",
        Description = "Microservicio de integración con SWAPI - Star Wars API",
        Contact = new()
        {
            Name = "Star Wars API Team"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StarWarsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Memory Cache
builder.Services.AddMemoryCache();

// HTTP Client for SWAPI
builder.Services.AddHttpClient<ISwapiService, SwapiClient>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Application Services
builder.Services.AddScoped<IFavoriteCharacterService, FavoriteCharacterService>();
builder.Services.AddScoped<IRequestHistoryService, RequestHistoryService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!)
    .AddCheck<SwapiHealthCheck>("swapi");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Star Wars API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StarWarsDbContext>();
    dbContext.Database.Migrate();
}

app.UseIpRateLimiting();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();


using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarWars.Api.Middleware;

/// <summary>
/// Health check personalizado para verificar la disponibilidad de SWAPI
/// </summary>
public class SwapiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SwapiHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync("https://swapi.dev/api/", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("SWAPI is reachable");
            }

            return HealthCheckResult.Degraded($"SWAPI returned status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SWAPI is unreachable", ex);
        }
    }
}


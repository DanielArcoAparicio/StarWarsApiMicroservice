using StarWars.Domain.Entities;

namespace StarWars.Application.Interfaces;

/// <summary>
/// Servicio para gestionar el historial de peticiones
/// </summary>
public interface IRequestHistoryService
{
    Task<List<ApiRequestHistory>> GetHistoryAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<ApiRequestHistory> LogRequestAsync(
        string endpoint,
        string method,
        int statusCode,
        long responseTimeMs,
        string? queryParameters = null,
        string? errorMessage = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetRequestStatisticsAsync(CancellationToken cancellationToken = default);
}


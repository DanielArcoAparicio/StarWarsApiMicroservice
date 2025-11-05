using Microsoft.EntityFrameworkCore;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;
using StarWars.Infrastructure.Data;

namespace StarWars.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar el historial de peticiones
/// </summary>
public class RequestHistoryService : IRequestHistoryService
{
    private readonly StarWarsDbContext _dbContext;

    public RequestHistoryService(StarWarsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ApiRequestHistory>> GetHistoryAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RequestHistory
            .OrderByDescending(h => h.RequestDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiRequestHistory> LogRequestAsync(
        string endpoint,
        string method,
        int statusCode,
        long responseTimeMs,
        string? queryParameters = null,
        string? errorMessage = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var history = new ApiRequestHistory
        {
            Endpoint = endpoint,
            Method = method,
            QueryParameters = queryParameters,
            StatusCode = statusCode,
            RequestDate = DateTime.UtcNow,
            ResponseTimeMs = responseTimeMs,
            ErrorMessage = errorMessage,
            IpAddress = ipAddress
        };

        _dbContext.RequestHistory.Add(history);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return history;
    }

    public async Task<Dictionary<string, int>> GetRequestStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _dbContext.RequestHistory
            .GroupBy(h => h.Endpoint)
            .Select(g => new { Endpoint = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .Take(10)
            .ToDictionaryAsync(s => s.Endpoint, s => s.Count, cancellationToken);

        return stats;
    }
}


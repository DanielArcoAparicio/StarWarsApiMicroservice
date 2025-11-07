using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;
using StarWars.Infrastructure.Data;

namespace StarWars.Infrastructure.Services;

/// <summary>
/// Servicio de caché multinivel (memoria + base de datos)
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly StarWarsDbContext _dbContext;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);

    public CacheService(IMemoryCache memoryCache, StarWarsDbContext dbContext)
    {
        _memoryCache = memoryCache;
        _dbContext = dbContext;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // Primero intenta obtener de memoria
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }

        // Si no está en memoria, busca en base de datos (con manejo de errores)
        try
        {
            var dbCache = await _dbContext.CachedData
                .FirstOrDefaultAsync(c => c.CacheKey == key && c.ExpirationDate > DateTime.UtcNow, cancellationToken);

            if (dbCache != null)
            {
                // Deserializar y guardar en memoria
                var value = JsonSerializer.Deserialize<T>(dbCache.Data);
                
                if (value != null)
                {
                    var expiration = dbCache.ExpirationDate - DateTime.UtcNow;
                    _memoryCache.Set(key, value, expiration);

                    // Actualizar estadísticas (con manejo de errores)
                    try
                    {
                        dbCache.AccessCount++;
                        dbCache.LastAccessDate = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                    catch
                    {
                        // Si falla guardar estadísticas, continuar sin actualizar
                    }

                    return value;
                }
            }
        }
        catch
        {
            // Si hay error con la BD, solo usar memoria cache (ya retornó null si no estaba en memoria)
            return null;
        }

        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var exp = expiration ?? _defaultExpiration;
        var expirationDate = DateTime.UtcNow.Add(exp);

        // Guardar en memoria (siempre funciona)
        _memoryCache.Set(key, value, exp);

        // Guardar en base de datos (con manejo de errores)
        try
        {
            var serializedData = JsonSerializer.Serialize(value);
            
            var existingCache = await _dbContext.CachedData
                .FirstOrDefaultAsync(c => c.CacheKey == key, cancellationToken);

            if (existingCache != null)
            {
                existingCache.Data = serializedData;
                existingCache.ExpirationDate = expirationDate;
                existingCache.LastAccessDate = DateTime.UtcNow;
            }
            else
            {
                _dbContext.CachedData.Add(new CachedData
                {
                    CacheKey = key,
                    Data = serializedData,
                    CreatedDate = DateTime.UtcNow,
                    ExpirationDate = expirationDate,
                    AccessCount = 0,
                    LastAccessDate = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Si falla guardar en BD, al menos está en memoria cache
            // No lanzar excepción para que el servicio continúe funcionando
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        var dbCache = await _dbContext.CachedData
            .FirstOrDefaultAsync(c => c.CacheKey == key, cancellationToken);

        if (dbCache != null)
        {
            _dbContext.CachedData.Remove(dbCache);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out _))
        {
            return true;
        }

        return await _dbContext.CachedData
            .AnyAsync(c => c.CacheKey == key && c.ExpirationDate > DateTime.UtcNow, cancellationToken);
    }

    public async Task ClearExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expiredCaches = await _dbContext.CachedData
            .Where(c => c.ExpirationDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredCaches.Any())
        {
            _dbContext.CachedData.RemoveRange(expiredCaches);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}


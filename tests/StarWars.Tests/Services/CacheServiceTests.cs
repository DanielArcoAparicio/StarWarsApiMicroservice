using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StarWars.Domain.Entities;
using StarWars.Domain.Models;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.Services;

namespace StarWars.Tests.Services;

public class CacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly StarWarsDbContext _dbContext;
    private readonly CacheService _service;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        var options = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new StarWarsDbContext(options);
        _service = new CacheService(_memoryCache, _dbContext);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Act
        var result = await _service.GetAsync<Character>("non_existent_key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_FromMemoryCache()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        _memoryCache.Set(key, character, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.GetAsync<Character>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task SetAsync_StoresValue_InMemoryCache()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };

        // Act
        await _service.SetAsync(key, character, TimeSpan.FromMinutes(10));

        // Assert
        var cached = _memoryCache.Get<Character>(key);
        cached.Should().NotBeNull();
        cached!.Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task SetAsync_StoresValue_InDatabase()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };

        // Act
        await _service.SetAsync(key, character, TimeSpan.FromMinutes(10));

        // Assert
        var dbCache = await _dbContext.CachedData.FirstOrDefaultAsync(c => c.CacheKey == key);
        dbCache.Should().NotBeNull();
        dbCache!.CacheKey.Should().Be(key);
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_FromDatabase_WhenNotInMemory()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        await _service.SetAsync(key, character, TimeSpan.FromMinutes(10));
        
        // Clear memory cache
        _memoryCache.Remove(key);

        // Act
        var result = await _service.GetAsync<Character>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Luke Skywalker");
        
        // Should also be back in memory cache
        var memoryCached = _memoryCache.Get<Character>(key);
        memoryCached.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAsync_RemovesFromMemoryAndDatabase()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        await _service.SetAsync(key, character, TimeSpan.FromMinutes(10));

        // Act
        await _service.RemoveAsync(key);

        // Assert
        var memoryCached = _memoryCache.Get<Character>(key);
        memoryCached.Should().BeNull();
        
        var dbCache = await _dbContext.CachedData.FirstOrDefaultAsync(c => c.CacheKey == key);
        dbCache.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenKeyExistsInMemory()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        _memoryCache.Set(key, character, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenKeyExistsInDatabase()
    {
        // Arrange
        var key = "test_key";
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        await _service.SetAsync(key, character, TimeSpan.FromMinutes(10));
        _memoryCache.Remove(key);

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenKeyDoesNotExist()
    {
        // Act
        var result = await _service.ExistsAsync("non_existent_key");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClearExpiredAsync_RemovesExpiredEntries()
    {
        // Arrange
        var expiredKey = "expired_key";
        var expiredCache = new CachedData
        {
            CacheKey = expiredKey,
            Data = "{}",
            CreatedDate = DateTime.UtcNow.AddDays(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            AccessCount = 0,
            LastAccessDate = DateTime.UtcNow.AddDays(-2)
        };
        _dbContext.CachedData.Add(expiredCache);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.ClearExpiredAsync();

        // Assert
        var exists = await _dbContext.CachedData.AnyAsync(c => c.CacheKey == expiredKey);
        exists.Should().BeFalse();
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        _dbContext.Dispose();
    }
}


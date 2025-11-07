using StarWars.Domain.Entities;

namespace StarWars.Tests.Domain;

public class CachedDataTests
{
    [Fact]
    public void CachedData_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var cachedData = new CachedData();

        // Assert
        cachedData.Should().NotBeNull();
        cachedData.CacheKey.Should().BeEmpty();
        cachedData.Data.Should().BeEmpty();
        cachedData.CreatedDate.Should().Be(default(DateTime));
        cachedData.ExpirationDate.Should().Be(default(DateTime));
        cachedData.AccessCount.Should().Be(0);
        cachedData.LastAccessDate.Should().Be(default(DateTime));
    }

    [Fact]
    public void CachedData_CanBeCreated_WithAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var cacheKey = "test_key";
        var data = "{\"test\":\"data\"}";
        var expirationDate = now.AddHours(1);

        // Act
        var cachedData = new CachedData
        {
            Id = 1,
            CacheKey = cacheKey,
            Data = data,
            CreatedDate = now,
            ExpirationDate = expirationDate,
            AccessCount = 5,
            LastAccessDate = now
        };

        // Assert
        cachedData.Should().NotBeNull();
        cachedData.Id.Should().Be(1);
        cachedData.CacheKey.Should().Be(cacheKey);
        cachedData.Data.Should().Be(data);
        cachedData.CreatedDate.Should().Be(now);
        cachedData.ExpirationDate.Should().Be(expirationDate);
        cachedData.AccessCount.Should().Be(5);
        cachedData.LastAccessDate.Should().Be(now);
    }

    [Fact]
    public void CachedData_Properties_CanBeModified()
    {
        // Arrange
        var cachedData = new CachedData
        {
            CacheKey = "initial_key",
            AccessCount = 0
        };

        // Act
        cachedData.CacheKey = "updated_key";
        cachedData.AccessCount = 10;
        cachedData.LastAccessDate = DateTime.UtcNow;

        // Assert
        cachedData.CacheKey.Should().Be("updated_key");
        cachedData.AccessCount.Should().Be(10);
        cachedData.LastAccessDate.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
    }
}


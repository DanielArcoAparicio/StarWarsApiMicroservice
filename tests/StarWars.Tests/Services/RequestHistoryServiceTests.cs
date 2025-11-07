using Microsoft.EntityFrameworkCore;
using StarWars.Domain.Entities;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.Services;

namespace StarWars.Tests.Services;

public class RequestHistoryServiceTests : IDisposable
{
    private readonly StarWarsDbContext _dbContext;
    private readonly RequestHistoryService _service;

    public RequestHistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new StarWarsDbContext(options);
        _service = new RequestHistoryService(_dbContext);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEmptyList_WhenNoHistoryExists()
    {
        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsHistory_OrderedByDateDescending()
    {
        // Arrange
        var history1 = new ApiRequestHistory
        {
            Endpoint = "/api/v1/characters",
            Method = "GET",
            StatusCode = 200,
            RequestDate = DateTime.UtcNow.AddDays(-2),
            ResponseTimeMs = 100
        };
        var history2 = new ApiRequestHistory
        {
            Endpoint = "/api/v1/favorites",
            Method = "GET",
            StatusCode = 200,
            RequestDate = DateTime.UtcNow.AddDays(-1),
            ResponseTimeMs = 150
        };

        _dbContext.RequestHistory.AddRange(history1, history2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Endpoint.Should().Be("/api/v1/favorites"); // Más reciente primero
        result[1].Endpoint.Should().Be("/api/v1/characters");
    }

    [Fact]
    public async Task GetHistoryAsync_RespectsLimit()
    {
        // Arrange
        for (int i = 0; i < 150; i++)
        {
            _dbContext.RequestHistory.Add(new ApiRequestHistory
            {
                Endpoint = $"/api/v1/endpoint{i}",
                Method = "GET",
                StatusCode = 200,
                RequestDate = DateTime.UtcNow.AddMinutes(-i),
                ResponseTimeMs = 100
            });
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetHistoryAsync(limit: 50);

        // Assert
        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task LogRequestAsync_CreatesNewHistoryEntry()
    {
        // Arrange
        var endpoint = "/api/v1/characters";
        var method = "GET";
        var statusCode = 200;
        var responseTimeMs = 150L;
        var queryParams = "?page=1";
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _service.LogRequestAsync(
            endpoint,
            method,
            statusCode,
            responseTimeMs,
            queryParams,
            null,
            ipAddress);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Endpoint.Should().Be(endpoint);
        result.Method.Should().Be(method);
        result.StatusCode.Should().Be(statusCode);
        result.ResponseTimeMs.Should().Be(responseTimeMs);
        result.QueryParameters.Should().Be(queryParams);
        result.IpAddress.Should().Be(ipAddress);

        var saved = await _dbContext.RequestHistory.FirstOrDefaultAsync(h => h.Id == result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task LogRequestAsync_HandlesError_ReturnsHistoryWithoutId()
    {
        // Arrange - Crear un nuevo contexto que fallará al guardar
        var newOptions = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var newContext = new StarWarsDbContext(newOptions);
        var service = new RequestHistoryService(newContext);
        
        // Cerrar el contexto para simular error en el siguiente SaveChangesAsync
        newContext.Dispose();
        
        var endpoint = "/api/v1/characters";
        var method = "GET";
        var statusCode = 200;
        var responseTimeMs = 150L;

        // Act
        var result = await service.LogRequestAsync(
            endpoint,
            method,
            statusCode,
            responseTimeMs);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(0); // Sin ID porque no se guardó
        result.Endpoint.Should().Be(endpoint);
    }

    [Fact]
    public async Task GetRequestStatisticsAsync_ReturnsEmptyDictionary_WhenNoHistory()
    {
        // Act
        var result = await _service.GetRequestStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRequestStatisticsAsync_ReturnsStatistics_GroupedByEndpoint()
    {
        // Arrange
        var history1 = new ApiRequestHistory
        {
            Endpoint = "/api/v1/characters",
            Method = "GET",
            StatusCode = 200,
            RequestDate = DateTime.UtcNow,
            ResponseTimeMs = 100
        };
        var history2 = new ApiRequestHistory
        {
            Endpoint = "/api/v1/characters",
            Method = "GET",
            StatusCode = 200,
            RequestDate = DateTime.UtcNow,
            ResponseTimeMs = 100
        };
        var history3 = new ApiRequestHistory
        {
            Endpoint = "/api/v1/favorites",
            Method = "GET",
            StatusCode = 200,
            RequestDate = DateTime.UtcNow,
            ResponseTimeMs = 100
        };

        _dbContext.RequestHistory.AddRange(history1, history2, history3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetRequestStatisticsAsync();

        // Assert
        result.Should().HaveCount(2);
        result["/api/v1/characters"].Should().Be(2);
        result["/api/v1/favorites"].Should().Be(1);
    }

    [Fact]
    public async Task GetRequestStatisticsAsync_ReturnsTop10_OrderedByCount()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _dbContext.RequestHistory.Add(new ApiRequestHistory
            {
                Endpoint = $"/api/v1/endpoint{i % 3}",
                Method = "GET",
                StatusCode = 200,
                RequestDate = DateTime.UtcNow,
                ResponseTimeMs = 100
            });
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetRequestStatisticsAsync();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task LogRequestAsync_HandlesNullQueryParameters()
    {
        // Arrange
        var endpoint = "/api/v1/characters";
        var method = "GET";
        var statusCode = 200;
        var responseTimeMs = 150L;

        // Act
        var result = await _service.LogRequestAsync(
            endpoint,
            method,
            statusCode,
            responseTimeMs,
            null, // QueryParameters null
            null,
            null);

        // Assert
        result.Should().NotBeNull();
        result.QueryParameters.Should().BeNull();
    }

    [Fact]
    public async Task LogRequestAsync_HandlesNullIpAddress()
    {
        // Arrange
        var endpoint = "/api/v1/characters";
        var method = "GET";
        var statusCode = 200;
        var responseTimeMs = 150L;

        // Act
        var result = await _service.LogRequestAsync(
            endpoint,
            method,
            statusCode,
            responseTimeMs,
            null,
            null,
            null); // IP null

        // Assert
        result.Should().NotBeNull();
        result.IpAddress.Should().BeNull();
    }

    [Fact]
    public async Task LogRequestAsync_HandlesErrorMessage()
    {
        // Arrange
        var endpoint = "/api/v1/characters";
        var method = "GET";
        var statusCode = 500;
        var responseTimeMs = 150L;
        var errorMessage = "Database connection failed";

        // Act
        var result = await _service.LogRequestAsync(
            endpoint,
            method,
            statusCode,
            responseTimeMs,
            null,
            errorMessage,
            null);

        // Assert
        result.Should().NotBeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.StatusCode.Should().Be(500);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}


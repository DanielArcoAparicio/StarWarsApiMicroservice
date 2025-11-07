using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarWars.Api.Controllers;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;

namespace StarWars.Tests.Controllers;

public class HistoryControllerTests
{
    private readonly Mock<IRequestHistoryService> _historyServiceMock;
    private readonly Mock<ILogger<HistoryController>> _loggerMock;
    private readonly HistoryController _controller;

    public HistoryControllerTests()
    {
        _historyServiceMock = new Mock<IRequestHistoryService>();
        _loggerMock = new Mock<ILogger<HistoryController>>();
        
        _controller = new HistoryController(
            _historyServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetHistory_ReturnsOk_WhenHistoryExists()
    {
        // Arrange
        var limit = 10;
        var history = new List<ApiRequestHistory>
        {
            new ApiRequestHistory
            {
                Id = 1,
                Endpoint = "/api/v1/characters",
                Method = "GET",
                StatusCode = 200,
                RequestDate = DateTime.UtcNow
            },
            new ApiRequestHistory
            {
                Id = 2,
                Endpoint = "/api/v1/favorites",
                Method = "GET",
                StatusCode = 200,
                RequestDate = DateTime.UtcNow
            }
        };

        _historyServiceMock.Setup(x => x.GetHistoryAsync(limit, default))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory(limit);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedHistory = okResult.Value.Should().BeOfType<List<ApiRequestHistory>>().Subject;
        returnedHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHistory_ReturnsDefaultLimit_WhenNoLimitSpecified()
    {
        // Arrange
        var defaultLimit = 100;
        var history = new List<ApiRequestHistory>();

        _historyServiceMock.Setup(x => x.GetHistoryAsync(defaultLimit, default))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        _historyServiceMock.Verify(x => x.GetHistoryAsync(defaultLimit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatistics_ReturnsOk_WithStatistics()
    {
        // Arrange
        var statistics = new Dictionary<string, int>
        {
            { "/api/v1/characters", 50 },
            { "/api/v1/favorites", 20 },
            { "/api/v1/history", 10 }
        };

        _historyServiceMock.Setup(x => x.GetRequestStatisticsAsync(default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<Dictionary<string, int>>().Subject;
        returnedStats.Should().HaveCount(3);
        returnedStats["/api/v1/characters"].Should().Be(50);
    }
}


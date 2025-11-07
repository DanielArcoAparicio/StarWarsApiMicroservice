using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarWars.Api.Middleware;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;

namespace StarWars.Tests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
    private readonly Mock<IRequestHistoryService> _historyServiceMock;
    private readonly RequestLoggingMiddleware _middleware;

    public RequestLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
        _historyServiceMock = new Mock<IRequestHistoryService>();
        
        RequestDelegate next = (HttpContext context) => Task.CompletedTask;
        _middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_IgnoresHealthCheck_DoesNotLog()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/health");
        context.Request.Method = "GET";
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_IgnoresSwagger_DoesNotLog()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/swagger/index.html");
        context.Request.Method = "GET";
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_LogsSuccessfulRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/v1/characters");
        context.Request.Method = "GET";
        context.Request.QueryString = new QueryString("?page=1");
        context.Response.StatusCode = 200;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        
        var history = new ApiRequestHistory
        {
            Id = 1,
            Endpoint = "/api/v1/characters?page=1",
            Method = "GET",
            StatusCode = 200
        };

        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<string>(e => e.Contains("/api/v1/characters")),
                "GET",
                200,
                It.IsAny<long>(),
                It.IsAny<string>(),
                null,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsFailedRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/v1/characters");
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        
        var exception = new Exception("Test error");
        var history = new ApiRequestHistory
        {
            Id = 1,
            Endpoint = "/api/v1/characters",
            Method = "GET",
            StatusCode = 500,
            ErrorMessage = "Test error"
        };

        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        
        RequestDelegate next = (HttpContext ctx) => throw exception;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context, _historyServiceMock.Object));

        _historyServiceMock.Verify(
            x => x.LogRequestAsync(
                It.IsAny<string>(),
                "GET",
                500,
                It.IsAny<long>(),
                It.IsAny<string>(),
                "Test error",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HandlesLoggingException_ContinuesWithoutFailing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/v1/characters");
        context.Request.Method = "GET";
        context.Response.StatusCode = 200;
        
        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert - Should not throw, just log warning
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_HandlesNullPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = PathString.Empty;
        context.Request.Method = "GET";
        context.Response.StatusCode = 200;
        
        var history = new ApiRequestHistory
        {
            Id = 1,
            Endpoint = "",
            Method = "GET",
            StatusCode = 200
        };

        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(
                It.IsAny<string>(),
                "GET",
                200,
                It.IsAny<long>(),
                It.IsAny<string>(),
                null,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HandlesNullRemoteIpAddress()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/v1/characters");
        context.Request.Method = "GET";
        context.Response.StatusCode = 200;
        context.Connection.RemoteIpAddress = null; // IP null
        
        var history = new ApiRequestHistory
        {
            Id = 1,
            Endpoint = "/api/v1/characters",
            Method = "GET",
            StatusCode = 200
        };

        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(
                It.IsAny<string>(),
                "GET",
                200,
                It.IsAny<long>(),
                It.IsAny<string>(),
                null,
                null, // IP debería ser null
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HandlesLoggingException_WhenRequestFails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/v1/characters");
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        
        var exception = new Exception("Test error");
        
        // Primera llamada (intento de logging del error) falla
        _historyServiceMock.Setup(x => x.LogRequestAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        RequestDelegate next = (HttpContext ctx) => throw exception;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context, _historyServiceMock.Object));
        
        // Verificar que se intentó registrar el error
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(
                It.IsAny<string>(),
                "GET",
                500,
                It.IsAny<long>(),
                It.IsAny<string>(),
                "Test error",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HandlesPathWithBothHealthAndSwagger()
    {
        // Arrange - Test branch coverage for path.Contains("/health") || path.Contains("/swagger")
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/api/health/swagger");
        context.Request.Method = "GET";
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert - Should not log because path contains both
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_HandlesPathWithOnlySwagger()
    {
        // Arrange - Test branch coverage for swagger path
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/swagger/ui");
        context.Request.Method = "GET";
        
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _historyServiceMock.Object);

        // Assert - Should not log
        _historyServiceMock.Verify(
            x => x.LogRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), 
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}


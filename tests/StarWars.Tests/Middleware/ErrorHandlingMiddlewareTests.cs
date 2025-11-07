using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarWars.Api.Middleware;

namespace StarWars.Tests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
    private readonly RequestDelegate _next;
    private readonly ErrorHandlingMiddleware _middleware;

    public ErrorHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _next = (HttpContext context) => Task.CompletedTask;
        _middleware = new ErrorHandlingMiddleware(_next, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_HandlesException_ReturnsErrorResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test exception");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Test exception");
    }

    [Fact]
    public async Task InvokeAsync_HandlesArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("Invalid argument");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_HandlesKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new KeyNotFoundException("Key not found");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_HandlesUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new UnauthorizedAccessException("Unauthorized");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_HandlesHttpRequestException_ReturnsServiceUnavailable()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new HttpRequestException("Service unavailable");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task InvokeAsync_HandlesHttpRequestExceptionWithStatusCode_ReturnsServiceUnavailable()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        // HttpRequestException no tiene una propiedad StatusCode directa, se maneja como ServiceUnavailable
        var exception = new HttpRequestException("Not found");
        
        RequestDelegate next = (HttpContext ctx) => throw exception;

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}


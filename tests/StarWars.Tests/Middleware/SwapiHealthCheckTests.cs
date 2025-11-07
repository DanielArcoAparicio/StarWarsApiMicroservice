using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq.Protected;
using StarWars.Api.Middleware;

namespace StarWars.Tests.Middleware;

// Wrapper para poder mockear CreateClient() sin parámetros
// El método de extensión CreateClient() sin parámetros llama a CreateClient(string.Empty)
public class TestableHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public TestableHttpClientFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}

public class SwapiHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenSwapiIsReachable()
    {
        // Arrange
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientFactory = new TestableHttpClientFactory(httpClient);
        var healthCheck = new SwapiHealthCheck(httpClientFactory);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("SWAPI is reachable");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDegraded_WhenSwapiReturnsError()
    {
        // Arrange
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientFactory = new TestableHttpClientFactory(httpClient);
        var healthCheck = new SwapiHealthCheck(httpClientFactory);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("SWAPI returned status code");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenSwapiIsUnreachable()
    {
        // Arrange
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientFactory = new TestableHttpClientFactory(httpClient);
        var healthCheck = new SwapiHealthCheck(httpClientFactory);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("SWAPI is unreachable");
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_HandlesTimeout()
    {
        // Arrange
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientFactory = new TestableHttpClientFactory(httpClient);
        var healthCheck = new SwapiHealthCheck(httpClientFactory);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }
}

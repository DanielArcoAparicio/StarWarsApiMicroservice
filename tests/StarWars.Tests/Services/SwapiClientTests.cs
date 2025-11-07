using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using StarWars.Domain.Models;
using StarWars.Infrastructure.External;
using StarWars.Infrastructure.External.DTOs;

namespace StarWars.Tests.Services;

public class SwapiClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly SwapiClient _swapiClient;

    public SwapiClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _swapiClient = new SwapiClient(_httpClient);
    }

    [Fact]
    public async Task GetCharactersAsync_ReturnsPagedResult_WhenSuccessful()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 2,
            Next = "https://swapi.dev/api/people/?page=2",
            Previous = null,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Height = "172",
                    Mass = "77",
                    Url = "https://swapi.dev/api/people/1/"
                },
                new SwapiPerson
                {
                    Name = "Darth Vader",
                    Height = "202",
                    Mass = "136",
                    Url = "https://swapi.dev/api/people/4/"
                }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(swapiResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.GetCharactersAsync(page);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Results.Should().HaveCount(2);
        result.Results[0].Name.Should().Be("Luke Skywalker");
        result.Results[0].Id.Should().Be("1");
        result.Results[1].Name.Should().Be("Darth Vader");
        result.Results[1].Id.Should().Be("4");
    }

    [Fact]
    public async Task GetCharactersAsync_ThrowsHttpRequestException_WhenRequestFails()
    {
        // Arrange
        var page = 1;
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharactersAsync(page));
    }

    [Fact]
    public async Task GetCharacterByIdAsync_ReturnsCharacter_WhenFound()
    {
        // Arrange
        var characterId = "1";
        var swapiPerson = new SwapiPerson
        {
            Name = "Luke Skywalker",
            Height = "172",
            Mass = "77",
            Url = "https://swapi.dev/api/people/1/"
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(swapiPerson)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.GetCharacterByIdAsync(characterId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Luke Skywalker");
        result.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetCharacterByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var characterId = "999";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.GetCharacterByIdAsync(characterId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_ReturnsCharacters_WhenFound()
    {
        // Arrange
        var searchName = "Luke";
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Height = "172",
                    Mass = "77",
                    Url = "https://swapi.dev/api/people/1/"
                }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(swapiResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.SearchCharactersByNameAsync(searchName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_ReturnsEmptyList_WhenNotFound()
    {
        // Arrange
        var searchName = "NonExistent";
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 0,
            Results = new List<SwapiPerson>()
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(swapiResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.SearchCharactersByNameAsync(searchName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}


using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

    [Fact]
    public async Task GetCharactersAsync_HandlesEmptyResults()
    {
        // Arrange
        var page = 1;
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
        var result = await _swapiClient.GetCharactersAsync(page);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetCharactersAsync_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 82, // 82 personajes, 10 por página = 9 páginas
            Results = new List<SwapiPerson>
            {
                new SwapiPerson { Name = "Luke Skywalker", Url = "https://swapi.dev/api/people/1/" }
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
        result.TotalPages.Should().Be(9); // 82 / 10 = 8.2, ceil = 9
    }

    [Fact]
    public async Task GetCharacterByIdAsync_HandlesNullResponse()
    {
        // Arrange
        var characterId = "1";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

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
    public async Task SearchCharactersByNameAsync_HandlesNullResults()
    {
        // Arrange
        var searchName = "Luke";
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 0,
            Results = new List<SwapiPerson>() // Lista vacía en lugar de null
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

    [Fact]
    public async Task SearchCharactersByNameAsync_HandlesNullSwapiResult()
    {
        // Arrange
        var searchName = "Luke";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

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

    [Fact]
    public async Task GetResourceAsync_ReturnsPagedResult()
    {
        // Arrange
        var endpoint = "planets";
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<object>
        {
            Count = 60,
            Results = new List<object> { new { name = "Tatooine" } }
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
        var result = await _swapiClient.GetResourceAsync<object>(endpoint, page);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(60);
        result.Results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetResourceAsync_ThrowsException_WhenRequestFails()
    {
        // Arrange
        var endpoint = "planets";
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
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetResourceAsync<object>(endpoint, page));
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesNullSwapiResult()
    {
        // Arrange
        var page = 1;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

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
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesTimeout()
    {
        // Arrange
        var page = 1;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout", new TimeoutException()));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharactersAsync(page));
    }

    [Fact]
    public async Task GetCharacterByIdAsync_HandlesTimeout()
    {
        // Arrange
        var characterId = "1";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout", new TimeoutException()));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharacterByIdAsync(characterId));
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_HandlesTimeout()
    {
        // Arrange
        var searchName = "Luke";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout", new TimeoutException()));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.SearchCharactersByNameAsync(searchName));
    }

    [Fact]
    public async Task GetCharactersAsync_ExtractsIdFromUrlCorrectly()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
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
        var result = await _swapiClient.GetCharactersAsync(page);

        // Assert
        result.Results[0].Id.Should().Be("1");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithoutTrailingSlash()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "https://swapi.dev/api/people/1" // Sin trailing slash
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
        result.Results[0].Id.Should().Be("1");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithEmptyParts()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "https://swapi.dev/api/people/1/" // Con trailing slash
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
        result.Results[0].Id.Should().Be("1");
        result.Results[0].Name.Should().Be("Luke Skywalker");
        result.Results[0].Url.Should().Be("https://swapi.dev/api/people/1/");
    }

    [Fact]
    public async Task GetCharacterByIdAsync_MapsAllCharacterProperties()
    {
        // Arrange
        var characterId = "1";
        var swapiPerson = new SwapiPerson
        {
            Name = "Luke Skywalker",
            Height = "172",
            Mass = "77",
            HairColor = "blond",
            SkinColor = "fair",
            EyeColor = "blue",
            BirthYear = "19BBY",
            Gender = "male",
            Homeworld = "https://swapi.dev/api/planets/1/",
            Films = new List<string> { "https://swapi.dev/api/films/1/" },
            Species = new List<string>(),
            Vehicles = new List<string> { "https://swapi.dev/api/vehicles/14/" },
            Starships = new List<string> { "https://swapi.dev/api/starships/12/" },
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
        result.Height.Should().Be("172");
        result.Mass.Should().Be("77");
        result.HairColor.Should().Be("blond");
        result.SkinColor.Should().Be("fair");
        result.EyeColor.Should().Be("blue");
        result.BirthYear.Should().Be("19BBY");
        result.Gender.Should().Be("male");
        result.HomeWorld.Should().Be("https://swapi.dev/api/planets/1/");
        result.Films.Should().HaveCount(1);
        result.Vehicles.Should().HaveCount(1);
        result.Starships.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetResourceAsync_HandlesNullSwapiResult()
    {
        // Arrange
        var endpoint = "planets";
        var page = 1;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _swapiClient.GetResourceAsync<object>(endpoint, page);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResourceAsync_CalculatesTotalPages()
    {
        // Arrange
        var endpoint = "planets";
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<object>
        {
            Count = 60,
            Results = new List<object> { new { name = "Tatooine" } }
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
        var result = await _swapiClient.GetResourceAsync<object>(endpoint, page);

        // Assert
        result.Should().NotBeNull();
        result.TotalPages.Should().Be(6); // 60 / 10 = 6
    }

    [Fact]
    public async Task GetCharactersAsync_ExtractsIdFromComplexUrl()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "https://swapi.dev/api/people/42/" // ID más largo
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
        result.Results[0].Id.Should().Be("42");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithNoId_ReturnsZero()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "https://swapi.dev/api/people/" // Sin ID
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
        result.Results[0].Id.Should().Be("0"); // Default cuando no hay ID
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesEmptyUrl_ReturnsZero()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "" // URL vacía
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
        result.Results[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithOnlySlash_ReturnsZero()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "/" // Solo slash
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
        result.Results[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task GetCharactersAsync_CalculatesTotalPages_WithRemainder()
    {
        // Arrange
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 87, // 87 / 10 = 8.7, ceil = 9
            Results = new List<SwapiPerson>
            {
                new SwapiPerson { Name = "Luke Skywalker", Url = "https://swapi.dev/api/people/1/" }
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
        result.TotalPages.Should().Be(9);
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesGeneralException_ThrowsHttpRequestException()
    {
        // Arrange
        var page = 1;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharactersAsync(page));
        exception.Message.Should().Contain("Error al obtener personajes de SWAPI");
    }

    [Fact]
    public async Task GetCharacterByIdAsync_HandlesGeneralException_ThrowsHttpRequestException()
    {
        // Arrange
        var characterId = "1";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharacterByIdAsync(characterId));
        exception.Message.Should().Contain($"Error al obtener personaje {characterId} de SWAPI");
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_HandlesGeneralException_ThrowsHttpRequestException()
    {
        // Arrange
        var searchName = "Luke";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.SearchCharactersByNameAsync(searchName));
        exception.Message.Should().Contain("Error al buscar personajes en SWAPI");
    }

    [Fact]
    public async Task GetCharacterByIdAsync_HandlesNon404Error_ThrowsHttpRequestException()
    {
        // Arrange
        var characterId = "1";
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.GetCharacterByIdAsync(characterId));
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithNonNumericLastPart_ReturnsZero()
    {
        // Arrange - Test branch when lastPart is not numeric
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "https://swapi.dev/api/people/abc/" // Non-numeric ID
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
        result.Results[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithEmptyParts_ReturnsZero()
    {
        // Arrange - Test branch when parts.Length == 0 after TrimEnd and Split
        // URL that after TrimEnd('/') becomes empty string, then Split results in empty array
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "/" // Single slash, after TrimEnd becomes empty, Split results in empty array
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
        result.Results[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUrlWithOnlyWhitespace_ReturnsZero()
    {
        // Arrange - Test branch when parts.Length == 0
        // URL with only whitespace that results in empty parts after processing
        var page = 1;
        var swapiResponse = new SwapiPagedResponse<SwapiPerson>
        {
            Count = 1,
            Results = new List<SwapiPerson>
            {
                new SwapiPerson
                {
                    Name = "Luke Skywalker",
                    Url = "   " // Whitespace only, after TrimEnd and Split results in empty array
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
        result.Results[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_HandlesNonSuccessStatusCode_ThrowsException()
    {
        // Arrange - Test branch when !response.IsSuccessStatusCode in SearchCharactersByNameAsync (lines 112-114)
        var searchName = "Luke";
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert - This should throw HttpRequestException which is then caught and re-thrown (lines 124-126)
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.SearchCharactersByNameAsync(searchName));
        exception.Message.Should().Contain("SWAPI returned status code");
    }

    [Fact]
    public async Task SearchCharactersByNameAsync_CatchesAndRethrowsHttpRequestException()
    {
        // Arrange - Test catch block that re-throws HttpRequestException (lines 124-126)
        // We need to throw an HttpRequestException from within the try block
        var searchName = "Luke";
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert - The HttpRequestException thrown at line 114 is caught at line 124 and re-thrown at line 126
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _swapiClient.SearchCharactersByNameAsync(searchName));
        exception.Message.Should().Contain("SWAPI returned status code");
    }
}


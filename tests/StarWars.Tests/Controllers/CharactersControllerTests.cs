using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarWars.Api.Controllers;
using StarWars.Application.Interfaces;
using StarWars.Domain.Models;

namespace StarWars.Tests.Controllers;

public class CharactersControllerTests
{
    private readonly Mock<ISwapiService> _swapiServiceMock;
    private readonly Mock<IFavoriteCharacterService> _favoriteServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CharactersController>> _loggerMock;
    private readonly CharactersController _controller;

    public CharactersControllerTests()
    {
        _swapiServiceMock = new Mock<ISwapiService>();
        _favoriteServiceMock = new Mock<IFavoriteCharacterService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CharactersController>>();
        
        _controller = new CharactersController(
            _swapiServiceMock.Object,
            _favoriteServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetCharacters_ReturnsOk_WhenCharactersExist()
    {
        // Arrange
        var page = 1;
        var expectedResult = new PagedResult<Character>
        {
            Count = 2,
            Page = page,
            TotalPages = 1,
            Results = new List<Character>
            {
                new Character { Id = "1", Name = "Luke Skywalker" },
                new Character { Id = "2", Name = "Darth Vader" }
            }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<Character>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<Character>>().Subject;
        pagedResult.Results.Should().HaveCount(2);
        pagedResult.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetCharacters_ReturnsCachedResult_WhenCacheExists()
    {
        // Arrange
        var page = 1;
        var cachedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker" } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<Character>>().Subject;
        pagedResult.Results.Should().HaveCount(1);
        _swapiServiceMock.Verify(x => x.GetCharactersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        // Cuando hay caché, no se actualiza el estado de favoritos (se retorna directamente)
    }

    [Fact]
    public async Task GetCharacters_ReturnsServiceUnavailable_WhenSwapiThrowsHttpRequestException()
    {
        // Arrange
        var page = 1;
        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SWAPI unavailable"));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetCharacterById_ReturnsOk_WhenCharacterExists()
    {
        // Arrange
        var characterId = "1";
        var character = new Character { Id = characterId, Name = "Luke Skywalker" };

        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(character);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Character>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCharacter = okResult.Value.Should().BeOfType<Character>().Subject;
        returnedCharacter.Id.Should().Be(characterId);
        returnedCharacter.Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task GetCharacterById_ReturnsNotFound_WhenCharacterDoesNotExist()
    {
        // Arrange
        var characterId = "999";
        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task SearchCharacters_ReturnsOk_WhenCharactersFound()
    {
        // Arrange
        var searchName = "Luke";
        var characters = new List<Character>
        {
            new Character { Id = "1", Name = "Luke Skywalker" }
        };

        _swapiServiceMock.Setup(x => x.SearchCharactersByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCharacters = okResult.Value.Should().BeOfType<List<Character>>().Subject;
        returnedCharacters.Should().HaveCount(1);
        returnedCharacters[0].Name.Should().Contain("Luke");
    }

    [Fact]
    public async Task SearchCharacters_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var searchName = "";

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task SearchCharacters_ReturnsServiceUnavailable_WhenSwapiThrowsHttpRequestException()
    {
        // Arrange
        var searchName = "Luke";
        _swapiServiceMock.Setup(x => x.SearchCharactersByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SWAPI unavailable"));

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetCharacters_Returns500_WhenResultIsNull()
    {
        // Arrange
        var page = 1;
        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>)null!);

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetCharacters_HandlesFavoriteServiceException_ContinuesWithFalse()
    {
        // Arrange
        var page = 1;
        var expectedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker" } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<Character>>().Subject;
        pagedResult.Results[0].IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task GetCharacters_HandlesCacheException_ContinuesWithoutCache()
    {
        // Arrange
        var page = 1;
        var expectedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker" } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<Character>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCharacterById_ReturnsCachedCharacter_WhenInCache()
    {
        // Arrange
        var characterId = "1";
        var cachedCharacter = new Character { Id = characterId, Name = "Luke Skywalker" };

        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCharacter);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var character = okResult.Value.Should().BeOfType<Character>().Subject;
        character.IsFavorite.Should().BeTrue();
        _swapiServiceMock.Verify(x => x.GetCharacterByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCharacterById_HandlesFavoriteServiceException_ContinuesWithFalse()
    {
        // Arrange
        var characterId = "1";
        var character = new Character { Id = characterId, Name = "Luke Skywalker" };

        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(character);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(characterId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCharacter = okResult.Value.Should().BeOfType<Character>().Subject;
        returnedCharacter.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCharacters_ReturnsEmptyList_WhenResultIsNull()
    {
        // Arrange
        var searchName = "NonExistent";
        _swapiServiceMock.Setup(x => x.SearchCharactersByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Character>)null!);

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var characters = okResult.Value.Should().BeOfType<List<Character>>().Subject;
        characters.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCharacters_HandlesResultsNull_Returns500()
    {
        // Arrange
        var page = 1;
        var expectedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = null!
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Character>?)null);
        _swapiServiceMock.Setup(x => x.GetCharactersAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetCharacterById_HandlesCacheException_ContinuesWithoutCache()
    {
        // Arrange
        var characterId = "1";
        var character = new Character { Id = characterId, Name = "Luke Skywalker" };

        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null); // Cache no tiene el valor, no lanza excepción
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(character);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Character>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error")); // Error al guardar en caché

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCharacters_ReturnsCachedResult_WithFavoriteStatus()
    {
        // Arrange
        var page = 1;
        var cachedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker", IsFavorite = false } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<Character>>().Subject;
        // El controlador ahora actualiza IsFavorite cuando hay caché
        pagedResult.Results[0].IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetCharacters_ReturnsCachedResult_WithoutCallingSwapi()
    {
        // Arrange
        var page = 1;
        var cachedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker" } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        _swapiServiceMock.Verify(x => x.GetCharactersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<Character>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCharacters_HandlesFavoriteServiceException_InCachedResult()
    {
        // Arrange
        var page = 1;
        var cachedResult = new PagedResult<Character>
        {
            Count = 1,
            Page = page,
            Results = new List<Character> { new Character { Id = "1", Name = "Luke Skywalker" } }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<Character>>().Subject;
        pagedResult.Results[0].IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task GetCharacters_HandlesGeneralException_Returns500()
    {
        // Arrange
        var page = 1;
        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<Character>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetCharacters(page);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetCharacterById_HandlesFavoriteServiceException_InCachedResult()
    {
        // Arrange
        var characterId = "1";
        var cachedCharacter = new Character { Id = characterId, Name = "Luke Skywalker" };

        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCharacter);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(characterId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var character = okResult.Value.Should().BeOfType<Character>().Subject;
        character.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task GetCharacterById_HandlesHttpRequestException_ReturnsServiceUnavailable()
    {
        // Arrange
        var characterId = "1";
        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SWAPI unavailable"));

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetCharacterById_HandlesGeneralException_Returns500()
    {
        // Arrange
        var characterId = "1";
        _cacheServiceMock.Setup(x => x.GetAsync<Character>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetCharacterById(characterId);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SearchCharacters_HandlesFavoriteServiceException_ContinuesWithFalse()
    {
        // Arrange
        var searchName = "Luke";
        var characters = new List<Character>
        {
            new Character { Id = "1", Name = "Luke Skywalker" }
        };

        _swapiServiceMock.Setup(x => x.SearchCharactersByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);
        _favoriteServiceMock.Setup(x => x.IsFavoriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCharacters = okResult.Value.Should().BeOfType<List<Character>>().Subject;
        returnedCharacters[0].IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCharacters_HandlesGeneralException_Returns500()
    {
        // Arrange
        var searchName = "Luke";
        _swapiServiceMock.Setup(x => x.SearchCharactersByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.SearchCharacters(searchName);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }
}


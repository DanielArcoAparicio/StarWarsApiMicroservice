using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarWars.Api.Controllers;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;
using StarWars.Domain.Models;

namespace StarWars.Tests.Controllers;

public class FavoritesControllerTests
{
    private readonly Mock<IFavoriteCharacterService> _favoriteServiceMock;
    private readonly Mock<ISwapiService> _swapiServiceMock;
    private readonly Mock<ILogger<FavoritesController>> _loggerMock;
    private readonly FavoritesController _controller;

    public FavoritesControllerTests()
    {
        _favoriteServiceMock = new Mock<IFavoriteCharacterService>();
        _swapiServiceMock = new Mock<ISwapiService>();
        _loggerMock = new Mock<ILogger<FavoritesController>>();
        
        _controller = new FavoritesController(
            _favoriteServiceMock.Object,
            _swapiServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetFavorites_ReturnsOk_WhenFavoritesExist()
    {
        // Arrange
        var favorites = new List<FavoriteCharacter>
        {
            new FavoriteCharacter { Id = 1, SwapiId = "1", Name = "Luke Skywalker" },
            new FavoriteCharacter { Id = 2, SwapiId = "2", Name = "Darth Vader" }
        };

        _favoriteServiceMock.Setup(x => x.GetAllFavoritesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorites);

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFavorites = okResult.Value.Should().BeOfType<List<FavoriteCharacter>>().Subject;
        returnedFavorites.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFavorites_ReturnsEmptyList_WhenNoFavoritesExist()
    {
        // Arrange
        _favoriteServiceMock.Setup(x => x.GetAllFavoritesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FavoriteCharacter>());

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFavorites = okResult.Value.Should().BeOfType<List<FavoriteCharacter>>().Subject;
        returnedFavorites.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFavoriteById_ReturnsOk_WhenFavoriteExists()
    {
        // Arrange
        var favoriteId = 1;
        var favorite = new FavoriteCharacter { Id = favoriteId, SwapiId = "1", Name = "Luke Skywalker" };

        _favoriteServiceMock.Setup(x => x.GetFavoriteByIdAsync(favoriteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorite);

        // Act
        var result = await _controller.GetFavoriteById(favoriteId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFavorite = okResult.Value.Should().BeOfType<FavoriteCharacter>().Subject;
        returnedFavorite.Id.Should().Be(favoriteId);
    }

    [Fact]
    public async Task GetFavoriteById_ReturnsNotFound_WhenFavoriteDoesNotExist()
    {
        // Arrange
        var favoriteId = 999;
        _favoriteServiceMock.Setup(x => x.GetFavoriteByIdAsync(favoriteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FavoriteCharacter?)null);

        // Act
        var result = await _controller.GetFavoriteById(favoriteId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddFavorite_ReturnsCreated_WhenCharacterExists()
    {
        // Arrange
        var request = new AddFavoriteRequest { CharacterId = "1", Notes = "My favorite" };
        var character = new Character { Id = "1", Name = "Luke Skywalker" };
        var favorite = new FavoriteCharacter { Id = 1, SwapiId = "1", Name = "Luke Skywalker", Notes = "My favorite" };

        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(request.CharacterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(character);
        _favoriteServiceMock.Setup(x => x.AddFavoriteAsync(character, request.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorite);

        // Act
        var result = await _controller.AddFavorite(request);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var returnedFavorite = createdResult.Value.Should().BeOfType<FavoriteCharacter>().Subject;
        returnedFavorite.SwapiId.Should().Be("1");
    }

    [Fact]
    public async Task AddFavorite_ReturnsBadRequest_WhenCharacterIdIsEmpty()
    {
        // Arrange
        var request = new AddFavoriteRequest { CharacterId = "", Notes = "My favorite" };

        // Act
        var result = await _controller.AddFavorite(request);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AddFavorite_ReturnsNotFound_WhenCharacterDoesNotExist()
    {
        // Arrange
        var request = new AddFavoriteRequest { CharacterId = "999", Notes = "My favorite" };
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(request.CharacterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Character?)null);

        // Act
        var result = await _controller.AddFavorite(request);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddFavorite_ReturnsServiceUnavailable_WhenSwapiThrowsHttpRequestException()
    {
        // Arrange
        var request = new AddFavoriteRequest { CharacterId = "1", Notes = "My favorite" };
        _swapiServiceMock.Setup(x => x.GetCharacterByIdAsync(request.CharacterId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SWAPI unavailable"));

        // Act
        var result = await _controller.AddFavorite(request);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task RemoveFavorite_ReturnsNoContent_WhenFavoriteExists()
    {
        // Arrange
        var favoriteId = 1;
        _favoriteServiceMock.Setup(x => x.RemoveFavoriteAsync(favoriteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFavorite(favoriteId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task RemoveFavorite_ReturnsNotFound_WhenFavoriteDoesNotExist()
    {
        // Arrange
        var favoriteId = 999;
        _favoriteServiceMock.Setup(x => x.RemoveFavoriteAsync(favoriteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveFavorite(favoriteId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task RemoveFavoriteBySwapiId_ReturnsNoContent_WhenFavoriteExists()
    {
        // Arrange
        var swapiId = "1";
        _favoriteServiceMock.Setup(x => x.RemoveFavoriteBySwapiIdAsync(swapiId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFavoriteBySwapiId(swapiId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task RemoveFavoriteBySwapiId_ReturnsNotFound_WhenFavoriteDoesNotExist()
    {
        // Arrange
        var swapiId = "999";
        _favoriteServiceMock.Setup(x => x.RemoveFavoriteBySwapiIdAsync(swapiId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveFavoriteBySwapiId(swapiId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }
}


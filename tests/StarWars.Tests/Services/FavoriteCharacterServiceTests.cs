using Microsoft.EntityFrameworkCore;
using StarWars.Domain.Entities;
using StarWars.Domain.Models;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.Services;

namespace StarWars.Tests.Services;

public class FavoriteCharacterServiceTests : IDisposable
{
    private readonly StarWarsDbContext _dbContext;
    private readonly FavoriteCharacterService _service;

    public FavoriteCharacterServiceTests()
    {
        var options = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new StarWarsDbContext(options);
        _service = new FavoriteCharacterService(_dbContext);
    }

    [Fact]
    public async Task GetAllFavoritesAsync_ReturnsEmptyList_WhenNoFavoritesExist()
    {
        // Act
        var result = await _service.GetAllFavoritesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllFavoritesAsync_ReturnsFavorites_OrderedByDateDescending()
    {
        // Arrange
        var favorite1 = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker",
            AddedDate = DateTime.UtcNow.AddDays(-2)
        };
        var favorite2 = new FavoriteCharacter
        {
            SwapiId = "2",
            Name = "Darth Vader",
            AddedDate = DateTime.UtcNow.AddDays(-1)
        };

        _dbContext.FavoriteCharacters.AddRange(favorite1, favorite2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFavoritesAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Darth Vader"); // Más reciente primero
        result[1].Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task GetFavoriteByIdAsync_ReturnsFavorite_WhenExists()
    {
        // Arrange
        var favorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFavoriteByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Luke Skywalker");
    }

    [Fact]
    public async Task GetFavoriteByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _service.GetFavoriteByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFavoriteBySwapiIdAsync_ReturnsFavorite_WhenExists()
    {
        // Arrange
        var favorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFavoriteBySwapiIdAsync("1");

        // Assert
        result.Should().NotBeNull();
        result!.SwapiId.Should().Be("1");
    }

    [Fact]
    public async Task AddFavoriteAsync_CreatesNewFavorite_WhenNotExists()
    {
        // Arrange
        var character = new Character
        {
            Id = "1",
            Name = "Luke Skywalker",
            Gender = "male",
            BirthYear = "19BBY",
            HomeWorld = "https://swapi.dev/api/planets/1/"
        };

        // Act
        var result = await _service.AddFavoriteAsync(character, "My favorite");

        // Assert
        result.Should().NotBeNull();
        result.SwapiId.Should().Be("1");
        result.Name.Should().Be("Luke Skywalker");
        result.Notes.Should().Be("My favorite");
        
        var savedFavorite = await _dbContext.FavoriteCharacters.FirstOrDefaultAsync(f => f.SwapiId == "1");
        savedFavorite.Should().NotBeNull();
    }

    [Fact]
    public async Task AddFavoriteAsync_ReturnsExisting_WhenAlreadyExists()
    {
        // Arrange
        var existingFavorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(existingFavorite);
        await _dbContext.SaveChangesAsync();

        var character = new Character
        {
            Id = "1",
            Name = "Luke Skywalker"
        };

        // Act
        var result = await _service.AddFavoriteAsync(character);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingFavorite.Id);
        
        var count = await _dbContext.FavoriteCharacters.CountAsync();
        count.Should().Be(1); // No se agregó uno nuevo
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ReturnsTrue_WhenFavoriteExists()
    {
        // Arrange
        var favorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveFavoriteAsync(favorite.Id);

        // Assert
        result.Should().BeTrue();
        var exists = await _dbContext.FavoriteCharacters.AnyAsync(f => f.Id == favorite.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ReturnsFalse_WhenFavoriteDoesNotExist()
    {
        // Act
        var result = await _service.RemoveFavoriteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFavoriteBySwapiIdAsync_ReturnsTrue_WhenFavoriteExists()
    {
        // Arrange
        var favorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveFavoriteBySwapiIdAsync("1");

        // Assert
        result.Should().BeTrue();
        var exists = await _dbContext.FavoriteCharacters.AnyAsync(f => f.SwapiId == "1");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task IsFavoriteAsync_ReturnsTrue_WhenFavoriteExists()
    {
        // Arrange
        var favorite = new FavoriteCharacter
        {
            SwapiId = "1",
            Name = "Luke Skywalker"
        };
        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsFavoriteAsync("1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFavoriteAsync_ReturnsFalse_WhenFavoriteDoesNotExist()
    {
        // Act
        var result = await _service.IsFavoriteAsync("999");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllFavoritesAsync_HandlesDatabaseError_ReturnsEmptyList()
    {
        // Arrange
        _dbContext.Dispose(); // Simular error de BD

        // Act
        var result = await _service.GetAllFavoritesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IsFavoriteAsync_HandlesDatabaseError_ReturnsFalse()
    {
        // Arrange
        _dbContext.Dispose(); // Simular error de BD

        // Act
        var result = await _service.IsFavoriteAsync("1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetFavoriteBySwapiIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _service.GetFavoriteBySwapiIdAsync("999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddFavoriteAsync_CreatesFavorite_WithAllProperties()
    {
        // Arrange
        var character = new Character
        {
            Id = "1",
            Name = "Luke Skywalker",
            Gender = "male",
            BirthYear = "19BBY",
            HomeWorld = "https://swapi.dev/api/planets/1/",
            Films = new List<string> { "https://swapi.dev/api/films/1/" },
            Species = new List<string>(),
            Vehicles = new List<string>(),
            Starships = new List<string>()
        };

        // Act
        var result = await _service.AddFavoriteAsync(character, "My favorite character");

        // Assert
        result.Should().NotBeNull();
        result.SwapiId.Should().Be("1");
        result.Name.Should().Be("Luke Skywalker");
        result.Gender.Should().Be("male");
        result.BirthYear.Should().Be("19BBY");
        result.HomeWorld.Should().Be("https://swapi.dev/api/planets/1/");
        result.Notes.Should().Be("My favorite character");
        result.AddedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddFavoriteAsync_CreatesFavorite_WithoutNotes()
    {
        // Arrange
        var character = new Character
        {
            Id = "1",
            Name = "Luke Skywalker"
        };

        // Act
        var result = await _service.AddFavoriteAsync(character);

        // Assert
        result.Should().NotBeNull();
        result.Notes.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFavoriteBySwapiIdAsync_ReturnsFalse_WhenFavoriteDoesNotExist()
    {
        // Act
        var result = await _service.RemoveFavoriteBySwapiIdAsync("999");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}


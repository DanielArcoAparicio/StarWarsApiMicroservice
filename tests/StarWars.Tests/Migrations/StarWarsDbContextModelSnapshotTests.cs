using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.Migrations;

namespace StarWars.Tests.Migrations;

public class StarWarsDbContextModelSnapshotTests : IDisposable
{
    private readonly StarWarsDbContext _dbContext;

    public StarWarsDbContextModelSnapshotTests()
    {
        var options = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new StarWarsDbContext(options);
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_ConfiguresApiRequestHistory()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        snapshot.Should().NotBeNull();
        
        var modelBuilder = new ModelBuilder();

        // Act - Execute BuildModel using reflection since it's protected
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod.Should().NotBeNull();
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert - Verify that the model was built
        var model = modelBuilder.Model;
        model.Should().NotBeNull();
        
        // Verify entities exist
        var entities = model.GetEntityTypes().ToList();
        entities.Should().Contain(e => e.Name.Contains("ApiRequestHistory"));
        entities.Should().Contain(e => e.Name.Contains("CachedData"));
        entities.Should().Contain(e => e.Name.Contains("FavoriteCharacter"));
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_ConfiguresCachedData()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        var modelBuilder = new ModelBuilder();

        // Act
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert
        var model = modelBuilder.Model;
        var cachedDataEntity = model.FindEntityType("StarWars.Domain.Entities.CachedData");
        cachedDataEntity.Should().NotBeNull();
        
        // Verify properties
        cachedDataEntity!.FindProperty("Id").Should().NotBeNull();
        cachedDataEntity.FindProperty("CacheKey").Should().NotBeNull();
        cachedDataEntity.FindProperty("Data").Should().NotBeNull();
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_ConfiguresFavoriteCharacter()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        var modelBuilder = new ModelBuilder();

        // Act
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert
        var model = modelBuilder.Model;
        var favoriteEntity = model.FindEntityType("StarWars.Domain.Entities.FavoriteCharacter");
        favoriteEntity.Should().NotBeNull();
        
        // Verify properties
        favoriteEntity!.FindProperty("Id").Should().NotBeNull();
        favoriteEntity.FindProperty("SwapiId").Should().NotBeNull();
        favoriteEntity.FindProperty("Name").Should().NotBeNull();
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_ConfiguresAllEntities()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        var modelBuilder = new ModelBuilder();

        // Act
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert - Verify all three entities are configured
        var model = modelBuilder.Model;
        var entities = model.GetEntityTypes().ToList();
        entities.Should().HaveCountGreaterThanOrEqualTo(3);
        
        var entityNames = entities.Select(e => e.Name).ToList();
        entityNames.Should().Contain("StarWars.Domain.Entities.ApiRequestHistory");
        entityNames.Should().Contain("StarWars.Domain.Entities.CachedData");
        entityNames.Should().Contain("StarWars.Domain.Entities.FavoriteCharacter");
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_ConfiguresIndexes()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        var modelBuilder = new ModelBuilder();

        // Act
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert - Verify indexes are configured
        var model = modelBuilder.Model;
        var cachedDataEntity = model.FindEntityType("StarWars.Domain.Entities.CachedData");
        var favoriteEntity = model.FindEntityType("StarWars.Domain.Entities.FavoriteCharacter");
        var historyEntity = model.FindEntityType("StarWars.Domain.Entities.ApiRequestHistory");
        
        cachedDataEntity.Should().NotBeNull();
        favoriteEntity.Should().NotBeNull();
        historyEntity.Should().NotBeNull();
        
        // Verify indexes exist (they are configured in the snapshot)
        var cachedDataIndexes = cachedDataEntity!.GetIndexes();
        var favoriteIndexes = favoriteEntity!.GetIndexes();
        var historyIndexes = historyEntity!.GetIndexes();
        
        cachedDataIndexes.Should().NotBeEmpty();
        favoriteIndexes.Should().NotBeEmpty();
        historyIndexes.Should().NotBeEmpty();
    }

    [Fact]
    public void StarWarsDbContextModelSnapshot_BuildModel_SetsCorrectAnnotations()
    {
        // Arrange
        var snapshotType = typeof(StarWarsDbContextModelSnapshot);
        var snapshot = Activator.CreateInstance(snapshotType);
        var modelBuilder = new ModelBuilder();

        // Act
        var buildModelMethod = snapshotType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Instance);
        buildModelMethod!.Invoke(snapshot, new object[] { modelBuilder });

        // Assert - Verify model annotations
        var model = modelBuilder.Model;
        model.Should().NotBeNull();
        
        // The snapshot should configure the model with proper annotations
        var productVersion = model.FindAnnotation("ProductVersion");
        productVersion.Should().NotBeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}


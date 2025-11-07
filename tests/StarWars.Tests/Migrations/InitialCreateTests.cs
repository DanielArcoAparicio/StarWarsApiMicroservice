using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StarWars.Infrastructure.Data;
using StarWars.Infrastructure.Migrations;

namespace StarWars.Tests.Migrations;

public class InitialCreateTests : IDisposable
{
    private readonly StarWarsDbContext _dbContext;

    public InitialCreateTests()
    {
        var options = new DbContextOptionsBuilder<StarWarsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new StarWarsDbContext(options);
    }

    [Fact]
    public void InitialCreate_Up_CreatesCachedDataTable()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");
        
        // Act - Execute Up method using reflection since it's protected
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod.Should().NotBeNull();
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify operations were added
        migrationBuilder.Operations.Should().NotBeEmpty();
        
        var createTableOps = migrationBuilder.Operations.OfType<CreateTableOperation>();
        createTableOps.Should().Contain(op => op.Name == "CachedData");
    }

    [Fact]
    public void InitialCreate_Up_CreatesFavoriteCharactersTable()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act - Execute Up method using reflection
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify CreateTable operation exists
        var createTableOps = migrationBuilder.Operations.OfType<CreateTableOperation>();
        createTableOps.Should().Contain(op => op.Name == "FavoriteCharacters");
    }

    [Fact]
    public void InitialCreate_Up_CreatesRequestHistoryTable()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert
        var createTableOps = migrationBuilder.Operations.OfType<CreateTableOperation>();
        createTableOps.Should().Contain(op => op.Name == "RequestHistory");
    }

    [Fact]
    public void InitialCreate_Up_CreatesIndexes()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify indexes are created
        var createIndexOps = migrationBuilder.Operations.OfType<CreateIndexOperation>();
        createIndexOps.Should().Contain(op => op.Name == "IX_CachedData_CacheKey");
        createIndexOps.Should().Contain(op => op.Name == "IX_CachedData_ExpirationDate");
        createIndexOps.Should().Contain(op => op.Name == "IX_FavoriteCharacters_SwapiId");
        createIndexOps.Should().Contain(op => op.Name == "IX_RequestHistory_Endpoint");
        createIndexOps.Should().Contain(op => op.Name == "IX_RequestHistory_RequestDate");
    }

    [Fact]
    public void InitialCreate_Down_DropsAllTables()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var downMethod = typeof(InitialCreate).GetMethod("Down", BindingFlags.NonPublic | BindingFlags.Instance);
        downMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify DropTable operations
        var dropTableOps = migrationBuilder.Operations.OfType<DropTableOperation>();
        dropTableOps.Should().Contain(op => op.Name == "CachedData");
        dropTableOps.Should().Contain(op => op.Name == "FavoriteCharacters");
        dropTableOps.Should().Contain(op => op.Name == "RequestHistory");
    }

    [Fact]
    public void InitialCreate_Up_CreatesAllRequiredTables()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify all three tables are created
        var createTableOps = migrationBuilder.Operations.OfType<CreateTableOperation>().ToList();
        createTableOps.Should().HaveCount(3);
        createTableOps.Should().Contain(op => op.Name == "CachedData");
        createTableOps.Should().Contain(op => op.Name == "FavoriteCharacters");
        createTableOps.Should().Contain(op => op.Name == "RequestHistory");
    }

    [Fact]
    public void InitialCreate_Up_CreatesCachedDataWithCorrectColumns()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify CachedData table structure
        var cachedDataTable = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .FirstOrDefault(op => op.Name == "CachedData");
        
        cachedDataTable.Should().NotBeNull();
        cachedDataTable!.Columns.Should().Contain(c => c.Name == "Id");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "CacheKey");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "Data");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "CreatedDate");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "ExpirationDate");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "AccessCount");
        cachedDataTable.Columns.Should().Contain(c => c.Name == "LastAccessDate");
    }

    [Fact]
    public void InitialCreate_Up_CreatesFavoriteCharactersWithCorrectColumns()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify FavoriteCharacters table structure
        var favoriteTable = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .FirstOrDefault(op => op.Name == "FavoriteCharacters");
        
        favoriteTable.Should().NotBeNull();
        favoriteTable!.Columns.Should().Contain(c => c.Name == "Id");
        favoriteTable.Columns.Should().Contain(c => c.Name == "SwapiId");
        favoriteTable.Columns.Should().Contain(c => c.Name == "Name");
        favoriteTable.Columns.Should().Contain(c => c.Name == "Gender");
        favoriteTable.Columns.Should().Contain(c => c.Name == "BirthYear");
        favoriteTable.Columns.Should().Contain(c => c.Name == "HomeWorld");
        favoriteTable.Columns.Should().Contain(c => c.Name == "AddedDate");
        favoriteTable.Columns.Should().Contain(c => c.Name == "Notes");
    }

    [Fact]
    public void InitialCreate_Up_CreatesRequestHistoryWithCorrectColumns()
    {
        // Arrange
        var migration = new InitialCreate();
        var migrationBuilder = new MigrationBuilder("TestProvider");

        // Act
        var upMethod = typeof(InitialCreate).GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });

        // Assert - Verify RequestHistory table structure
        var historyTable = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .FirstOrDefault(op => op.Name == "RequestHistory");
        
        historyTable.Should().NotBeNull();
        historyTable!.Columns.Should().Contain(c => c.Name == "Id");
        historyTable.Columns.Should().Contain(c => c.Name == "Endpoint");
        historyTable.Columns.Should().Contain(c => c.Name == "Method");
        historyTable.Columns.Should().Contain(c => c.Name == "QueryParameters");
        historyTable.Columns.Should().Contain(c => c.Name == "StatusCode");
        historyTable.Columns.Should().Contain(c => c.Name == "RequestDate");
        historyTable.Columns.Should().Contain(c => c.Name == "ResponseTimeMs");
        historyTable.Columns.Should().Contain(c => c.Name == "ErrorMessage");
        historyTable.Columns.Should().Contain(c => c.Name == "IpAddress");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}


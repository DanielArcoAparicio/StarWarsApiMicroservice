using Microsoft.EntityFrameworkCore;
using StarWars.Domain.Entities;

namespace StarWars.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para la aplicación Star Wars
/// </summary>
public class StarWarsDbContext : DbContext
{
    public StarWarsDbContext(DbContextOptions<StarWarsDbContext> options)
        : base(options)
    {
    }

    public DbSet<FavoriteCharacter> FavoriteCharacters { get; set; }
    public DbSet<ApiRequestHistory> RequestHistory { get; set; }
    public DbSet<CachedData> CachedData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de FavoriteCharacter
        modelBuilder.Entity<FavoriteCharacter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SwapiId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.BirthYear).HasMaxLength(50);
            entity.Property(e => e.HomeWorld).HasMaxLength(200);
            entity.Property(e => e.AddedDate).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => e.SwapiId).IsUnique();
        });

        // Configuración de ApiRequestHistory
        modelBuilder.Entity<ApiRequestHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.QueryParameters).HasMaxLength(1000);
            entity.Property(e => e.RequestDate).IsRequired();
            entity.Property(e => e.StatusCode).IsRequired();
            entity.Property(e => e.ResponseTimeMs).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.HasIndex(e => e.RequestDate);
            entity.HasIndex(e => e.Endpoint);
        });

        // Configuración de CachedData
        modelBuilder.Entity<CachedData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CacheKey).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ExpirationDate).IsRequired();
            entity.Property(e => e.AccessCount).IsRequired();
            entity.Property(e => e.LastAccessDate).IsRequired();
            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.ExpirationDate);
        });
    }
}


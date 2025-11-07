using Microsoft.EntityFrameworkCore;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;
using StarWars.Domain.Models;
using StarWars.Infrastructure.Data;

namespace StarWars.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar personajes favoritos
/// </summary>
public class FavoriteCharacterService : IFavoriteCharacterService
{
    private readonly StarWarsDbContext _dbContext;

    public FavoriteCharacterService(StarWarsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FavoriteCharacter>> GetAllFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.FavoriteCharacters
                .OrderByDescending(f => f.AddedDate)
                .ToListAsync(cancellationToken);
        }
        catch
        {
            // Si hay error con la BD, retornar lista vacía en lugar de fallar
            return new List<FavoriteCharacter>();
        }
    }

    public async Task<FavoriteCharacter?> GetFavoriteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FavoriteCharacters
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FavoriteCharacter?> GetFavoriteBySwapiIdAsync(string swapiId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FavoriteCharacters
            .FirstOrDefaultAsync(f => f.SwapiId == swapiId, cancellationToken);
    }

    public async Task<FavoriteCharacter> AddFavoriteAsync(Character character, string? notes = null, CancellationToken cancellationToken = default)
    {
        // Verificar si ya existe
        var existing = await GetFavoriteBySwapiIdAsync(character.Id, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var favorite = new FavoriteCharacter
        {
            SwapiId = character.Id,
            Name = character.Name,
            Gender = character.Gender,
            BirthYear = character.BirthYear,
            HomeWorld = character.HomeWorld,
            AddedDate = DateTime.UtcNow,
            Notes = notes
        };

        _dbContext.FavoriteCharacters.Add(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return favorite;
    }

    public async Task<bool> RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default)
    {
        var favorite = await GetFavoriteByIdAsync(id, cancellationToken);
        if (favorite == null)
        {
            return false;
        }

        _dbContext.FavoriteCharacters.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFavoriteBySwapiIdAsync(string swapiId, CancellationToken cancellationToken = default)
    {
        var favorite = await GetFavoriteBySwapiIdAsync(swapiId, cancellationToken);
        if (favorite == null)
        {
            return false;
        }

        _dbContext.FavoriteCharacters.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsFavoriteAsync(string swapiId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.FavoriteCharacters
                .AnyAsync(f => f.SwapiId == swapiId, cancellationToken);
        }
        catch
        {
            // Si hay error con la BD, retornar false (no es favorito) en lugar de fallar
            return false;
        }
    }
}


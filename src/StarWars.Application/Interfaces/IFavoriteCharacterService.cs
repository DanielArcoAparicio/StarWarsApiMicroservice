using StarWars.Domain.Entities;
using StarWars.Domain.Models;

namespace StarWars.Application.Interfaces;

/// <summary>
/// Servicio para gestionar personajes favoritos
/// </summary>
public interface IFavoriteCharacterService
{
    Task<List<FavoriteCharacter>> GetAllFavoritesAsync(CancellationToken cancellationToken = default);
    Task<FavoriteCharacter?> GetFavoriteByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FavoriteCharacter?> GetFavoriteBySwapiIdAsync(string swapiId, CancellationToken cancellationToken = default);
    Task<FavoriteCharacter> AddFavoriteAsync(Character character, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteBySwapiIdAsync(string swapiId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(string swapiId, CancellationToken cancellationToken = default);
}


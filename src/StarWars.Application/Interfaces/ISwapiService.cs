using StarWars.Domain.Models;

namespace StarWars.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de integración con SWAPI
/// </summary>
public interface ISwapiService
{
    Task<PagedResult<Character>> GetCharactersAsync(int page = 1, CancellationToken cancellationToken = default);
    Task<Character?> GetCharacterByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Character>> SearchCharactersByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PagedResult<T>> GetResourceAsync<T>(string endpoint, int page = 1, CancellationToken cancellationToken = default);
}


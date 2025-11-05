using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using StarWars.Client.Models;

namespace StarWars.Client;

/// <summary>
/// Cliente HTTP para interactuar con la API de Star Wars
/// </summary>
public class StarWarsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public StarWarsApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // Characters
    public async Task<PagedResult<Character>?> GetCharactersAsync(int page = 1)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<Character>>(
                $"/api/v1/characters?page={page}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener personajes: {ex.Message}");
            return null;
        }
    }

    public async Task<Character?> GetCharacterByIdAsync(string id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Character>(
                $"/api/v1/characters/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener personaje: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Character>?> SearchCharactersAsync(string name)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Character>>(
                $"/api/v1/characters/search?name={Uri.EscapeDataString(name)}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar personajes: {ex.Message}");
            return null;
        }
    }

    // Favorites
    public async Task<List<FavoriteCharacter>?> GetFavoritesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<FavoriteCharacter>>(
                "/api/v1/favorites", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener favoritos: {ex.Message}");
            return null;
        }
    }

    public async Task<FavoriteCharacter?> AddFavoriteAsync(string characterId, string? notes = null)
    {
        try
        {
            var request = new { CharacterId = characterId, Notes = notes };
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/v1/favorites", content);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FavoriteCharacter>(json, _jsonOptions);
            }

            Console.WriteLine($"Error al agregar favorito: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al agregar favorito: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/favorites/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar favorito: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFavoriteBySwapiIdAsync(string swapiId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/favorites/character/{swapiId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar favorito: {ex.Message}");
            return false;
        }
    }

    // History
    public async Task<List<ApiRequestHistory>?> GetHistoryAsync(int limit = 50)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ApiRequestHistory>>(
                $"/api/v1/history?limit={limit}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener historial: {ex.Message}");
            return null;
        }
    }

    public async Task<Dictionary<string, int>?> GetStatisticsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, int>>(
                "/api/v1/history/statistics", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas: {ex.Message}");
            return null;
        }
    }
}


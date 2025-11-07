using System.Net.Http.Json;
using System.Text.Json;
using StarWars.Application.Interfaces;
using StarWars.Domain.Models;
using StarWars.Infrastructure.External.DTOs;

namespace StarWars.Infrastructure.External;

/// <summary>
/// Cliente para interactuar con la API de Star Wars (SWAPI)
/// </summary>
public class SwapiClient : ISwapiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public SwapiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://swapi.dev/api/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "StarWars-Microservice");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PagedResult<Character>> GetCharactersAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"people/?page={page}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"SWAPI returned status code: {response.StatusCode}");
            }

            var swapiResult = await response.Content.ReadFromJsonAsync<SwapiPagedResponse<SwapiPerson>>(_jsonOptions, cancellationToken);
            
            if (swapiResult == null)
                return new PagedResult<Character>();

            var characters = swapiResult.Results.Select(MapToCharacter).ToList();

            return new PagedResult<Character>
            {
                Count = swapiResult.Count,
                Next = swapiResult.Next,
                Previous = swapiResult.Previous,
                Results = characters,
                Page = page,
                TotalPages = (int)Math.Ceiling(swapiResult.Count / 10.0)
            };
        }
        catch (HttpRequestException)
        {
            throw; // Re-lanzar para que el controlador lo maneje
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new HttpRequestException("Timeout al conectar con SWAPI", ex);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Error al obtener personajes de SWAPI", ex);
        }
    }

    public async Task<Character?> GetCharacterByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"people/{id}/", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                // Si es 404, retornar null (personaje no encontrado)
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                // Para otros errores, lanzar excepción
                throw new HttpRequestException($"SWAPI returned status code: {response.StatusCode}");
            }

            var swapiPerson = await response.Content.ReadFromJsonAsync<SwapiPerson>(_jsonOptions, cancellationToken);
            
            return swapiPerson != null ? MapToCharacter(swapiPerson) : null;
        }
        catch (HttpRequestException)
        {
            throw; // Re-lanzar para que el controlador lo maneje
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new HttpRequestException("Timeout al conectar con SWAPI", ex);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Error al obtener personaje {id} de SWAPI", ex);
        }
    }

    public async Task<List<Character>> SearchCharactersByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"people/?search={Uri.EscapeDataString(name)}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"SWAPI returned status code: {response.StatusCode}");
            }

            var swapiResult = await response.Content.ReadFromJsonAsync<SwapiPagedResponse<SwapiPerson>>(_jsonOptions, cancellationToken);
            
            if (swapiResult == null || swapiResult.Results.Count == 0)
                return new List<Character>();

            return swapiResult.Results.Select(MapToCharacter).ToList();
        }
        catch (HttpRequestException)
        {
            throw; // Re-lanzar para que el controlador lo maneje
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new HttpRequestException("Timeout al conectar con SWAPI", ex);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Error al buscar personajes en SWAPI", ex);
        }
    }

    public async Task<PagedResult<T>> GetResourceAsync<T>(string endpoint, int page = 1, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{endpoint}/?page={page}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var swapiResult = await response.Content.ReadFromJsonAsync<SwapiPagedResponse<T>>(_jsonOptions, cancellationToken);
        
        if (swapiResult == null)
            return new PagedResult<T>();

        return new PagedResult<T>
        {
            Count = swapiResult.Count,
            Next = swapiResult.Next,
            Previous = swapiResult.Previous,
            Results = swapiResult.Results,
            Page = page,
            TotalPages = (int)Math.Ceiling(swapiResult.Count / 10.0)
        };
    }

    private Character MapToCharacter(SwapiPerson person)
    {
        var id = ExtractIdFromUrl(person.Url);
        
        return new Character
        {
            Id = id,
            Name = person.Name,
            Height = person.Height,
            Mass = person.Mass,
            HairColor = person.HairColor,
            SkinColor = person.SkinColor,
            EyeColor = person.EyeColor,
            BirthYear = person.BirthYear,
            Gender = person.Gender,
            HomeWorld = person.Homeworld,
            Films = person.Films,
            Species = person.Species,
            Vehicles = person.Vehicles,
            Starships = person.Starships,
            Url = person.Url
        };
    }

    private string ExtractIdFromUrl(string url)
    {
        var parts = url.TrimEnd('/').Split('/');
        return parts.Length > 0 ? parts[^1] : "0";
    }
}


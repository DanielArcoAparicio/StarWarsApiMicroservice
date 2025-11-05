using System.Text.Json.Serialization;

namespace StarWars.Infrastructure.External.DTOs;

/// <summary>
/// Respuesta paginada genérica de SWAPI
/// </summary>
public class SwapiPagedResponse<T>
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = new();
}


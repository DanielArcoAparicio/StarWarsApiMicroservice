namespace StarWars.Domain.Models;

/// <summary>
/// Resultado paginado genérico
/// </summary>
public class PagedResult<T>
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<T> Results { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
}


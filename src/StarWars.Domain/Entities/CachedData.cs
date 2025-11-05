namespace StarWars.Domain.Entities;

/// <summary>
/// Almacena datos en caché de forma persistente en la base de datos
/// </summary>
public class CachedData
{
    public int Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int AccessCount { get; set; }
    public DateTime LastAccessDate { get; set; }
}


namespace StarWars.Domain.Entities;

/// <summary>
/// Representa un personaje favorito guardado por el usuario
/// </summary>
public class FavoriteCharacter
{
    public int Id { get; set; }
    public string SwapiId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string BirthYear { get; set; } = string.Empty;
    public string HomeWorld { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public string? Notes { get; set; }
}


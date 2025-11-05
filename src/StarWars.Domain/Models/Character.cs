namespace StarWars.Domain.Models;

/// <summary>
/// Modelo de dominio para un personaje de Star Wars
/// </summary>
public class Character
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string Mass { get; set; } = string.Empty;
    public string HairColor { get; set; } = string.Empty;
    public string SkinColor { get; set; } = string.Empty;
    public string EyeColor { get; set; } = string.Empty;
    public string BirthYear { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string HomeWorld { get; set; } = string.Empty;
    public List<string> Films { get; set; } = new();
    public List<string> Species { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
}


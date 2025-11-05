namespace StarWars.Client.Models;

public class ApiRequestHistory
{
    public int Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? QueryParameters { get; set; }
    public int StatusCode { get; set; }
    public DateTime RequestDate { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
}


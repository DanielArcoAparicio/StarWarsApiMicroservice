using StarWars.Client;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║          Star Wars API - Cliente de Consola                  ║");
Console.WriteLine("║          Integración con SWAPI (Star Wars API)                ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configurar la URL base de la API
Console.Write("Ingrese la URL base de la API (por defecto: http://localhost:5000): ");
var baseUrl = Console.ReadLine();
if (string.IsNullOrWhiteSpace(baseUrl))
{
    baseUrl = "http://localhost:5000";
}

var client = new StarWarsApiClient(baseUrl);
var menu = new ConsoleMenu(client);

await menu.ShowMainMenuAsync();


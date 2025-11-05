namespace StarWars.Client;

/// <summary>
/// Menú de consola interactivo para la aplicación cliente
/// </summary>
public class ConsoleMenu
{
    private readonly StarWarsApiClient _client;

    public ConsoleMenu(StarWarsApiClient client)
    {
        _client = client;
    }

    public async Task ShowMainMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine("              MENÚ PRINCIPAL - STAR WARS API                ");
            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("1. Listar personajes");
            Console.WriteLine("2. Buscar personaje por nombre");
            Console.WriteLine("3. Ver detalles de un personaje");
            Console.WriteLine("4. Ver favoritos");
            Console.WriteLine("5. Agregar personaje a favoritos");
            Console.WriteLine("6. Eliminar personaje de favoritos");
            Console.WriteLine("7. Ver historial de peticiones");
            Console.WriteLine("8. Ver estadísticas");
            Console.WriteLine("0. Salir");
            Console.WriteLine();
            Console.Write("Seleccione una opción: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await ListCharactersAsync();
                    break;
                case "2":
                    await SearchCharactersAsync();
                    break;
                case "3":
                    await GetCharacterDetailsAsync();
                    break;
                case "4":
                    await ViewFavoritesAsync();
                    break;
                case "5":
                    await AddFavoriteAsync();
                    break;
                case "6":
                    await RemoveFavoriteAsync();
                    break;
                case "7":
                    await ViewHistoryAsync();
                    break;
                case "8":
                    await ViewStatisticsAsync();
                    break;
                case "0":
                    Console.WriteLine("\n¡Hasta luego! Que la Fuerza te acompañe.");
                    return;
                default:
                    Console.WriteLine("\nOpción inválida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private async Task ListCharactersAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("                    LISTAR PERSONAJES                       ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.Write("Ingrese el número de página (por defecto 1): ");
        var pageInput = Console.ReadLine();
        var page = string.IsNullOrWhiteSpace(pageInput) ? 1 : int.Parse(pageInput);

        var result = await _client.GetCharactersAsync(page);

        if (result == null || result.Results.Count == 0)
        {
            Console.WriteLine("\nNo se encontraron personajes.");
        }
        else
        {
            Console.WriteLine($"\nTotal de personajes: {result.Count}");
            Console.WriteLine($"Página {result.Page} de {result.TotalPages}");
            Console.WriteLine();

            foreach (var character in result.Results)
            {
                var favorite = character.IsFavorite ? " ★" : "";
                Console.WriteLine($"[{character.Id}] {character.Name}{favorite}");
                Console.WriteLine($"    Género: {character.Gender}, Año de nacimiento: {character.BirthYear}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task SearchCharactersAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("                 BUSCAR PERSONAJES                          ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.Write("Ingrese el nombre a buscar: ");
        var name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("\nDebe ingresar un nombre.");
        }
        else
        {
            var characters = await _client.SearchCharactersAsync(name);

            if (characters == null || characters.Count == 0)
            {
                Console.WriteLine("\nNo se encontraron personajes con ese nombre.");
            }
            else
            {
                Console.WriteLine($"\nSe encontraron {characters.Count} personaje(s):");
                Console.WriteLine();

                foreach (var character in characters)
                {
                    var favorite = character.IsFavorite ? " ★" : "";
                    Console.WriteLine($"[{character.Id}] {character.Name}{favorite}");
                    Console.WriteLine($"    Género: {character.Gender}");
                    Console.WriteLine($"    Año de nacimiento: {character.BirthYear}");
                    Console.WriteLine($"    Altura: {character.Height} cm, Peso: {character.Mass} kg");
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task GetCharacterDetailsAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("                DETALLES DE PERSONAJE                       ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.Write("Ingrese el ID del personaje: ");
        var id = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(id))
        {
            Console.WriteLine("\nDebe ingresar un ID.");
        }
        else
        {
            var character = await _client.GetCharacterByIdAsync(id);

            if (character == null)
            {
                Console.WriteLine("\nPersonaje no encontrado.");
            }
            else
            {
                var favorite = character.IsFavorite ? " ★ (Favorito)" : "";
                Console.WriteLine($"\nNombre: {character.Name}{favorite}");
                Console.WriteLine($"Género: {character.Gender}");
                Console.WriteLine($"Año de nacimiento: {character.BirthYear}");
                Console.WriteLine($"Altura: {character.Height} cm");
                Console.WriteLine($"Peso: {character.Mass} kg");
                Console.WriteLine($"Color de cabello: {character.HairColor}");
                Console.WriteLine($"Color de piel: {character.SkinColor}");
                Console.WriteLine($"Color de ojos: {character.EyeColor}");
                Console.WriteLine($"Películas: {character.Films.Count}");
                Console.WriteLine($"Vehículos: {character.Vehicles.Count}");
                Console.WriteLine($"Naves: {character.Starships.Count}");
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task ViewFavoritesAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("                 PERSONAJES FAVORITOS                       ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        var favorites = await _client.GetFavoritesAsync();

        if (favorites == null || favorites.Count == 0)
        {
            Console.WriteLine("No tienes personajes favoritos guardados.");
        }
        else
        {
            Console.WriteLine($"Total de favoritos: {favorites.Count}\n");

            foreach (var favorite in favorites)
            {
                Console.WriteLine($"[{favorite.Id}] ★ {favorite.Name}");
                Console.WriteLine($"    SWAPI ID: {favorite.SwapiId}");
                Console.WriteLine($"    Género: {favorite.Gender}");
                Console.WriteLine($"    Año de nacimiento: {favorite.BirthYear}");
                Console.WriteLine($"    Agregado: {favorite.AddedDate:dd/MM/yyyy HH:mm}");
                if (!string.IsNullOrWhiteSpace(favorite.Notes))
                {
                    Console.WriteLine($"    Notas: {favorite.Notes}");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task AddFavoriteAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("              AGREGAR PERSONAJE A FAVORITOS                 ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.Write("Ingrese el ID del personaje: ");
        var characterId = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(characterId))
        {
            Console.WriteLine("\nDebe ingresar un ID.");
        }
        else
        {
            Console.Write("Ingrese notas (opcional): ");
            var notes = Console.ReadLine();

            var favorite = await _client.AddFavoriteAsync(characterId, notes);

            if (favorite != null)
            {
                Console.WriteLine($"\n✓ {favorite.Name} agregado a favoritos exitosamente!");
            }
            else
            {
                Console.WriteLine("\n✗ No se pudo agregar el personaje a favoritos.");
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task RemoveFavoriteAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("            ELIMINAR PERSONAJE DE FAVORITOS                 ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        // Mostrar favoritos primero
        var favorites = await _client.GetFavoritesAsync();

        if (favorites == null || favorites.Count == 0)
        {
            Console.WriteLine("No tienes personajes favoritos guardados.");
        }
        else
        {
            Console.WriteLine("Tus favoritos:\n");
            foreach (var fav in favorites)
            {
                Console.WriteLine($"[{fav.Id}] {fav.Name}");
            }
            Console.WriteLine();

            Console.Write("Ingrese el ID del favorito a eliminar: ");
            var idInput = Console.ReadLine();

            if (int.TryParse(idInput, out int id))
            {
                var success = await _client.RemoveFavoriteAsync(id);

                if (success)
                {
                    Console.WriteLine("\n✓ Favorito eliminado exitosamente!");
                }
                else
                {
                    Console.WriteLine("\n✗ No se pudo eliminar el favorito.");
                }
            }
            else
            {
                Console.WriteLine("\nID inválido.");
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task ViewHistoryAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("              HISTORIAL DE PETICIONES                       ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.Write("Ingrese el número de registros a mostrar (por defecto 20): ");
        var limitInput = Console.ReadLine();
        var limit = string.IsNullOrWhiteSpace(limitInput) ? 20 : int.Parse(limitInput);

        var history = await _client.GetHistoryAsync(limit);

        if (history == null || history.Count == 0)
        {
            Console.WriteLine("No hay historial de peticiones.");
        }
        else
        {
            Console.WriteLine($"Últimas {history.Count} peticiones:\n");

            foreach (var record in history)
            {
                var status = record.StatusCode >= 200 && record.StatusCode < 300 ? "✓" : "✗";
                Console.WriteLine($"{status} [{record.Method}] {record.Endpoint}");
                Console.WriteLine($"   Status: {record.StatusCode} | Tiempo: {record.ResponseTimeMs}ms");
                Console.WriteLine($"   Fecha: {record.RequestDate:dd/MM/yyyy HH:mm:ss}");
                if (!string.IsNullOrWhiteSpace(record.ErrorMessage))
                {
                    Console.WriteLine($"   Error: {record.ErrorMessage}");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task ViewStatisticsAsync()
    {
        Console.Clear();
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine("                    ESTADÍSTICAS                            ");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        Console.WriteLine();

        var stats = await _client.GetStatisticsAsync();

        if (stats == null || stats.Count == 0)
        {
            Console.WriteLine("No hay estadísticas disponibles.");
        }
        else
        {
            Console.WriteLine("Top 10 endpoints más utilizados:\n");

            var sorted = stats.OrderByDescending(s => s.Value);
            var position = 1;

            foreach (var stat in sorted)
            {
                Console.WriteLine($"{position}. {stat.Key}");
                Console.WriteLine($"   Peticiones: {stat.Value}");
                Console.WriteLine();
                position++;
            }
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }
}


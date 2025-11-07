using Microsoft.AspNetCore.Mvc;
using StarWars.Application.Interfaces;
using StarWars.Domain.Models;

namespace StarWars.Api.Controllers;

/// <summary>
/// Controlador para gestionar personajes de Star Wars
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CharactersController : ControllerBase
{
    private readonly ISwapiService _swapiService;
    private readonly IFavoriteCharacterService _favoriteService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CharactersController> _logger;

    public CharactersController(
        ISwapiService swapiService,
        IFavoriteCharacterService favoriteService,
        ICacheService cacheService,
        ILogger<CharactersController> logger)
    {
        _swapiService = swapiService;
        _favoriteService = favoriteService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una lista paginada de personajes
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <returns>Lista paginada de personajes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Character>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<Character>>> GetCharacters([FromQuery] int page = 1)
    {
        try
        {
            _logger.LogInformation("Obteniendo personajes - Página {Page}", page);

            var cacheKey = $"characters_page_{page}";
            var cachedResult = await _cacheService.GetAsync<PagedResult<Character>>(cacheKey);

            if (cachedResult != null)
            {
                _logger.LogInformation("Personajes obtenidos desde caché");
                // Actualizar estado de favoritos (con manejo de errores)
                foreach (var character in cachedResult.Results)
                {
                    try
                    {
                        character.IsFavorite = await _favoriteService.IsFavoriteAsync(character.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al verificar si el personaje {Id} es favorito", character.Id);
                        character.IsFavorite = false;
                    }
                }
                return Ok(cachedResult);
            }

            var result = await _swapiService.GetCharactersAsync(page);

            if (result == null || result.Results == null)
            {
                _logger.LogWarning("No se obtuvieron resultados de SWAPI");
                return StatusCode(500, new { message = "Error al obtener personajes de SWAPI" });
            }

            // Marcar favoritos (con manejo de errores para no fallar si la BD no está disponible)
            foreach (var character in result.Results)
            {
                try
                {
                    character.IsFavorite = await _favoriteService.IsFavoriteAsync(character.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al verificar si el personaje {Id} es favorito", character.Id);
                    character.IsFavorite = false; // Valor por defecto si falla
                }
            }

            try
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar en caché, continuando sin caché");
            }

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al conectar con SWAPI");
            return StatusCode(503, new { message = "Servicio SWAPI no disponible", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener personajes");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un personaje por su ID
    /// </summary>
    /// <param name="id">ID del personaje</param>
    /// <returns>Detalles del personaje</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Character), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Character>> GetCharacterById(string id)
    {
        try
        {
            _logger.LogInformation("Obteniendo personaje con ID {Id}", id);

            var cacheKey = $"character_{id}";
            var cachedCharacter = await _cacheService.GetAsync<Character>(cacheKey);

            if (cachedCharacter != null)
            {
                _logger.LogInformation("Personaje obtenido desde caché");
                try
                {
                    cachedCharacter.IsFavorite = await _favoriteService.IsFavoriteAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al verificar si el personaje {Id} es favorito", id);
                    cachedCharacter.IsFavorite = false;
                }
                return Ok(cachedCharacter);
            }

            var character = await _swapiService.GetCharacterByIdAsync(id);

            if (character == null)
            {
                _logger.LogWarning("Personaje con ID {Id} no encontrado en SWAPI", id);
                return NotFound(new { message = $"Personaje con ID {id} no encontrado" });
            }

            try
            {
                character.IsFavorite = await _favoriteService.IsFavoriteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar si el personaje {Id} es favorito", id);
                character.IsFavorite = false;
            }

            try
            {
                await _cacheService.SetAsync(cacheKey, character, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar en caché, continuando sin caché");
            }

            return Ok(character);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al conectar con SWAPI para obtener personaje {Id}", id);
            return StatusCode(503, new { message = "Servicio SWAPI no disponible", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener personaje {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Busca personajes por nombre
    /// </summary>
    /// <param name="name">Nombre o parte del nombre a buscar</param>
    /// <returns>Lista de personajes que coinciden con la búsqueda</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<Character>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Character>>> SearchCharacters([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "El parámetro 'name' es requerido" });
            }

            _logger.LogInformation("Buscando personajes con nombre: {Name}", name);

            var characters = await _swapiService.SearchCharactersByNameAsync(name);

            if (characters == null)
            {
                _logger.LogWarning("No se obtuvieron resultados de SWAPI para la búsqueda: {Name}", name);
                return Ok(new List<Character>()); // Retornar lista vacía en lugar de error
            }

            // Marcar favoritos (con manejo de errores para no fallar si la BD no está disponible)
            foreach (var character in characters)
            {
                try
                {
                    character.IsFavorite = await _favoriteService.IsFavoriteAsync(character.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al verificar si el personaje {Id} es favorito", character.Id);
                    character.IsFavorite = false; // Valor por defecto si falla
                }
            }

            return Ok(characters);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al conectar con SWAPI para búsqueda: {Name}", name);
            return StatusCode(503, new { message = "Servicio SWAPI no disponible", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar personajes: {Name}", name);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}


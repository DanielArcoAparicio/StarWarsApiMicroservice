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
        _logger.LogInformation("Obteniendo personajes - Página {Page}", page);

        var cacheKey = $"characters_page_{page}";
        var cachedResult = await _cacheService.GetAsync<PagedResult<Character>>(cacheKey);

        if (cachedResult != null)
        {
            _logger.LogInformation("Personajes obtenidos desde caché");
            return Ok(cachedResult);
        }

        var result = await _swapiService.GetCharactersAsync(page);

        // Marcar favoritos
        foreach (var character in result.Results)
        {
            character.IsFavorite = await _favoriteService.IsFavoriteAsync(character.Id);
        }

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un personaje por su ID
    /// </summary>
    /// <param name="id">ID del personaje</param>
    /// <returns>Detalles del personaje</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Character), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Character>> GetCharacterById(string id)
    {
        _logger.LogInformation("Obteniendo personaje con ID {Id}", id);

        var cacheKey = $"character_{id}";
        var cachedCharacter = await _cacheService.GetAsync<Character>(cacheKey);

        if (cachedCharacter != null)
        {
            _logger.LogInformation("Personaje obtenido desde caché");
            cachedCharacter.IsFavorite = await _favoriteService.IsFavoriteAsync(id);
            return Ok(cachedCharacter);
        }

        var character = await _swapiService.GetCharacterByIdAsync(id);

        if (character == null)
        {
            return NotFound(new { message = $"Personaje con ID {id} no encontrado" });
        }

        character.IsFavorite = await _favoriteService.IsFavoriteAsync(id);
        await _cacheService.SetAsync(cacheKey, character, TimeSpan.FromHours(1));

        return Ok(character);
    }

    /// <summary>
    /// Busca personajes por nombre
    /// </summary>
    /// <param name="name">Nombre o parte del nombre a buscar</param>
    /// <returns>Lista de personajes que coinciden con la búsqueda</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<Character>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Character>>> SearchCharacters([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "El parámetro 'name' es requerido" });
        }

        _logger.LogInformation("Buscando personajes con nombre: {Name}", name);

        var characters = await _swapiService.SearchCharactersByNameAsync(name);

        // Marcar favoritos
        foreach (var character in characters)
        {
            character.IsFavorite = await _favoriteService.IsFavoriteAsync(character.Id);
        }

        return Ok(characters);
    }
}


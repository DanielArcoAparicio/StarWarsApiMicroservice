using Microsoft.AspNetCore.Mvc;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;
using StarWars.Domain.Models;

namespace StarWars.Api.Controllers;

/// <summary>
/// Controlador para gestionar personajes favoritos
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteCharacterService _favoriteService;
    private readonly ISwapiService _swapiService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoriteCharacterService favoriteService,
        ISwapiService swapiService,
        ILogger<FavoritesController> logger)
    {
        _favoriteService = favoriteService;
        _swapiService = swapiService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los personajes favoritos
    /// </summary>
    /// <returns>Lista de personajes favoritos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<FavoriteCharacter>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FavoriteCharacter>>> GetFavorites()
    {
        _logger.LogInformation("Obteniendo todos los favoritos");
        var favorites = await _favoriteService.GetAllFavoritesAsync();
        return Ok(favorites);
    }

    /// <summary>
    /// Obtiene un favorito por su ID
    /// </summary>
    /// <param name="id">ID del favorito</param>
    /// <returns>Detalles del favorito</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FavoriteCharacter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteCharacter>> GetFavoriteById(int id)
    {
        var favorite = await _favoriteService.GetFavoriteByIdAsync(id);
        
        if (favorite == null)
        {
            return NotFound(new { message = $"Favorito con ID {id} no encontrado" });
        }

        return Ok(favorite);
    }

    /// <summary>
    /// Agrega un personaje a favoritos
    /// </summary>
    /// <param name="request">Información del personaje a agregar</param>
    /// <returns>Personaje agregado a favoritos</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FavoriteCharacter), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteCharacter>> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CharacterId))
        {
            return BadRequest(new { message = "CharacterId es requerido" });
        }

        _logger.LogInformation("Agregando personaje {CharacterId} a favoritos", request.CharacterId);

        // Verificar si el personaje existe en SWAPI
        var character = await _swapiService.GetCharacterByIdAsync(request.CharacterId);
        
        if (character == null)
        {
            return NotFound(new { message = $"Personaje con ID {request.CharacterId} no encontrado en SWAPI" });
        }

        var favorite = await _favoriteService.AddFavoriteAsync(character, request.Notes);

        return CreatedAtAction(
            nameof(GetFavoriteById),
            new { id = favorite.Id },
            favorite);
    }

    /// <summary>
    /// Elimina un personaje de favoritos por su ID
    /// </summary>
    /// <param name="id">ID del favorito</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        _logger.LogInformation("Eliminando favorito con ID {Id}", id);

        var removed = await _favoriteService.RemoveFavoriteAsync(id);

        if (!removed)
        {
            return NotFound(new { message = $"Favorito con ID {id} no encontrado" });
        }

        return NoContent();
    }

    /// <summary>
    /// Elimina un personaje de favoritos por su SWAPI ID
    /// </summary>
    /// <param name="swapiId">ID de SWAPI del personaje</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("character/{swapiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavoriteBySwapiId(string swapiId)
    {
        _logger.LogInformation("Eliminando favorito con SWAPI ID {SwapiId}", swapiId);

        var removed = await _favoriteService.RemoveFavoriteBySwapiIdAsync(swapiId);

        if (!removed)
        {
            return NotFound(new { message = $"Favorito con SWAPI ID {swapiId} no encontrado" });
        }

        return NoContent();
    }
}

/// <summary>
/// Request para agregar un favorito
/// </summary>
public class AddFavoriteRequest
{
    /// <summary>
    /// ID del personaje en SWAPI
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// Notas opcionales sobre el personaje
    /// </summary>
    public string? Notes { get; set; }
}


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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<FavoriteCharacter>>> GetFavorites()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los favoritos");
            var favorites = await _favoriteService.GetAllFavoritesAsync();
            return Ok(favorites ?? new List<FavoriteCharacter>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener favoritos");
            return StatusCode(500, new { message = "Error al obtener favoritos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un favorito por su ID
    /// </summary>
    /// <param name="id">ID del favorito</param>
    /// <returns>Detalles del favorito</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FavoriteCharacter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FavoriteCharacter>> GetFavoriteById(int id)
    {
        try
        {
            var favorite = await _favoriteService.GetFavoriteByIdAsync(id);
            
            if (favorite == null)
            {
                return NotFound(new { message = $"Favorito con ID {id} no encontrado" });
            }

            return Ok(favorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener favorito con ID {Id}", id);
            return StatusCode(500, new { message = "Error al obtener favorito", error = ex.Message });
        }
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FavoriteCharacter>> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CharacterId))
            {
                return BadRequest(new { message = "CharacterId es requerido" });
            }

            _logger.LogInformation("Agregando personaje {CharacterId} a favoritos", request.CharacterId);

            // Verificar si el personaje existe en SWAPI
            Character? character;
            try
            {
                character = await _swapiService.GetCharacterByIdAsync(request.CharacterId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al conectar con SWAPI para verificar personaje {CharacterId}", request.CharacterId);
                return StatusCode(503, new { message = "Servicio SWAPI no disponible", error = ex.Message });
            }
            
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar favorito para personaje {CharacterId}", request.CharacterId);
            return StatusCode(500, new { message = "Error al agregar favorito", error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un personaje de favoritos por su ID
    /// </summary>
    /// <param name="id">ID del favorito</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando favorito con ID {Id}", id);

            var removed = await _favoriteService.RemoveFavoriteAsync(id);

            if (!removed)
            {
                return NotFound(new { message = $"Favorito con ID {id} no encontrado" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar favorito con ID {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar favorito", error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un personaje de favoritos por su SWAPI ID
    /// </summary>
    /// <param name="swapiId">ID de SWAPI del personaje</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("character/{swapiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFavoriteBySwapiId(string swapiId)
    {
        try
        {
            _logger.LogInformation("Eliminando favorito con SWAPI ID {SwapiId}", swapiId);

            var removed = await _favoriteService.RemoveFavoriteBySwapiIdAsync(swapiId);

            if (!removed)
            {
                return NotFound(new { message = $"Favorito con SWAPI ID {swapiId} no encontrado" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar favorito con SWAPI ID {SwapiId}", swapiId);
            return StatusCode(500, new { message = "Error al eliminar favorito", error = ex.Message });
        }
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


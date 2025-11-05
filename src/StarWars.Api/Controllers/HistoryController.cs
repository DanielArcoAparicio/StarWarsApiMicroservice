using Microsoft.AspNetCore.Mvc;
using StarWars.Application.Interfaces;
using StarWars.Domain.Entities;

namespace StarWars.Api.Controllers;

/// <summary>
/// Controlador para gestionar el historial de peticiones
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class HistoryController : ControllerBase
{
    private readonly IRequestHistoryService _historyService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(
        IRequestHistoryService historyService,
        ILogger<HistoryController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el historial de peticiones
    /// </summary>
    /// <param name="limit">Número máximo de registros a retornar (por defecto 100)</param>
    /// <returns>Lista de peticiones históricas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiRequestHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApiRequestHistory>>> GetHistory([FromQuery] int limit = 100)
    {
        _logger.LogInformation("Obteniendo historial de peticiones (límite: {Limit})", limit);
        
        var history = await _historyService.GetHistoryAsync(limit);
        return Ok(history);
    }

    /// <summary>
    /// Obtiene estadísticas de las peticiones
    /// </summary>
    /// <returns>Estadísticas agrupadas por endpoint</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatistics()
    {
        _logger.LogInformation("Obteniendo estadísticas de peticiones");
        
        var stats = await _historyService.GetRequestStatisticsAsync();
        return Ok(stats);
    }
}


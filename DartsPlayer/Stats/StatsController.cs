
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DartsPlayer.Stats;

[Authorize]
[ApiController]
[Route("/stats")]
public class StatsController : ControllerBase
{
    private StatsService _statsService;
    public StatsController(StatsService statsService)
    {
        _statsService = statsService;
    }
    
    /// <summary>
    /// Retrieves the statistics for a specific player identified by their player ID.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the statistics for the player with the specified ID. The statistics include average score,
    /// matches played, doubles hit, all double attempts, and max high score
    /// </remarks>
    /// <param name="playerId">The unique identifier of the player whose statistics are to be retrieved.</param>
    /// <returns>A list of statistics for the specified player, or a 404 Not Found response if no statistics are available.</returns>
    /// <response code="200">Returns the statistics for the specified player.</response>
    /// <response code="404">No statistics are found for the specified player.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{playerId}")]
    public async Task<IActionResult> GetStats(Guid playerId)
    {
        try
        {
            var stats = await _statsService.GetAllStats(playerId);
            if (stats == null)
            {
                return NotFound(new { message = "Match not found" });
            }

            return Ok(new { stats });

        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }

    
}

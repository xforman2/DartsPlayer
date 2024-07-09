using DartsPlayer.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsPlayer.Player;
[Authorize]
[ApiController]
[Route("/players")]
public class PlayerController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    public PlayerController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a list of all users managed by the application.
    /// </remarks>
    /// <returns>A list of all users, or an empty list if no users are found.</returns>
    /// <response code="200">Returns a list of all users.</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(new { users });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error"});
        }
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the user with the specified ID.
    /// </remarks>
    /// <param name="playerId">The ID of the user to retrieve.</param>
    /// <returns>The user with the specified ID</returns>
    /// <response code="200">User information of the user with the specified ID.</response>
    /// <response code="404">No user is found with the specified ID.</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{playerId}")]
    public async Task<IActionResult> GetUserById(Guid playerId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(playerId.ToString());
            if (user == null)
            {
                return NotFound(new { error = $"User with ID {playerId} not found." });
            }

            return Ok( new { user });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
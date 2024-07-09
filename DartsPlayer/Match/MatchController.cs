using DartsPlayer.Match.Models;
using DartsPlayer.Match.Requests;
using DartsPlayer.Match.Validators;
using DartsPlayer.Player.Models;
using DartsPlayer.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DartsPlayer.Match;
[Authorize]
[ApiController]
[Route("/matches")]
public class MatchController: ControllerBase
{
    private MatchService _matchService;
    private UserManager<ApplicationUser> _userManager;
    private ActiveMatchService _activeMatchService;
    private UpdateScoreValidator _updateScoreValidator;
    private MatchTypeValidator _matchTypeValidator;
    
    public MatchController(MatchService matchService,
        UserManager<ApplicationUser> userManager, 
        ActiveMatchService activeMatchService,
        UpdateScoreValidator updateScoreValidator, 
        MatchTypeValidator matchTypeValidator)
    {
        _matchService = matchService;
        _userManager = userManager;
        _activeMatchService = activeMatchService;
        _updateScoreValidator = updateScoreValidator;
        _matchTypeValidator = matchTypeValidator;
    }
    
    /// <summary>
    /// Retrieves matches based on the type parameter.
    /// </summary>
    /// <remarks>
    /// This endpoint returns matches based on the type parameter.
    /// The type parameter can be 'Joinable' = 0, 'InProgress' = 1, or 'Ended' = 2.
    /// </remarks>
    /// <param name="type">The type of matches to retrieve.</param>
    /// <returns>A list of matches based on the type parameter.</returns>
    /// <response code="200">Returns the requested matches.</response>
    /// <response code="400">Returns an error if the type parameter is invalid.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    public async Task<IActionResult> GetMatches([FromQuery] MatchState type)
    {
        try
        {
            if (type == MatchState.Joinable)
            {
                return Ok(new { _activeMatchService.ActiveMaches });
            }

            if (type == MatchState.Ended)
            {
                var matches = await _matchService.GetAllMatches();
                return Ok(new { matches });
            }

            if (type == MatchState.InProgress)
            {
                return Ok(new { _activeMatchService.PlayedMatches });
            }

            return BadRequest(new { error = "Invalid type parameter" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }

    /// <summary>
    /// Joins a match for the current user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a user to join an existing match by its ID.
    /// </remarks>
    /// <param name="matchId">The ID of the match to join.</param>
    /// <returns>A success response if the operation is successful, otherwise an error response.</returns>
    /// <response code="200">Match joined successfully.</response>
    /// <response code="400">The match does not exist or the user is already in a match or match is full.</response>
    /// <response code="401">Unauthorized if the user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{matchId}/join")]
    public async Task<IActionResult> JoinMatch(Guid matchId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }
            if (!_activeMatchService.ActiveMaches.TryGetValue(matchId, out var match))
            {
                return NotFound(new { error = "Match not found" });
            }

            var result = match.JoinMatch(new PlayerModel(currentUser.Id));
            if (result.IsError)
            {
                return BadRequest(new { error = result.FirstError.Code });
            }

            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }
    
    /// <summary>
    /// Starts a match created by the current user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a user to start an existing match by its ID.
    /// </remarks>
    /// <param name="matchId">The ID of the match to start.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Match started successfully.</response>
    /// <response code="400">The match does not exist or the user is not authorized to start it.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{matchId}/start")]
    public async Task<IActionResult> StartMatch(Guid matchId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!_activeMatchService.ActiveMaches.TryGetValue(matchId, out var match))
            {
                return NotFound(new { error = "Match not found" });
            }

            var result = match.StartMatch(currentUser);
            if (result.IsError)
            {
                return BadRequest(new {error = result.FirstError.Code});
            }

            _activeMatchService.ActiveMaches.Remove(matchId);
            _activeMatchService.PlayedMatches.Add(matchId, match);
            return Ok( new { match });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }
    
    /// <summary>
    /// Creates a new match.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a user to create a new match with a specified type. Type determines the starting score of
    /// both players at the start of the match ( Possible values are: 501, 301, 101)
    /// </remarks>
    /// <param name="request">The request containing the match type.</param>
    /// <returns>The ID of the newly created match.</returns>
    /// <response code="200">Created a new match.</response>
    /// <response code="400">The request is invalid or the user is already in a match.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] MatchTypeRequest request)
    {
        try
        {
            var validationResult = await _matchTypeValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (_activeMatchService.PlayerHasMatch(currentUser.Id))
            {
                return BadRequest(new { error = "User can only have one active match"});
            }

            var player = new PlayerModel(currentUser.Id);
            var matchId = Guid.NewGuid();

            _activeMatchService.ActiveMaches.Add(matchId, new ActiveMatchModel(player, request.MatchType));

            return Ok(new { matchId });

        }
        catch (Exception)
        {   
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Updates the score of a match.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a user to update the score of a match they are currently playing in.
    /// </remarks>
    /// <param name="matchId">The ID of the match to update.</param>
    /// <param name="body">The request containing the updated score.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Score updated successfully or match has ended</response>
    /// <response code="400">The request is invalid or the user is not authorized to update the score.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{matchId}/score")]
    public async Task<IActionResult> UpdateScore(Guid matchId, [FromBody] UpdateScoreRequest body)
    {
        try
        {
            var validationResult = await _updateScoreValidator.ValidateAsync(body);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!_activeMatchService.PlayedMatches.TryGetValue(matchId, out var match))
            {
                return NotFound(new { error = "Match not found" });
            }

            if (match.Turn.Id != user.Id)
            {
                return Unauthorized(new { error = "Not your turn" });
            }

            match.UpdateScore(body.Throws);

            if (match.isFinished())
            {
                var result = await _matchService.AddMatch(match);
                if (result.IsError)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                _activeMatchService.PlayedMatches.Remove(matchId);
                return Ok(new { message = "Match has ended" });
            }

            return Ok(new { message = "Score was updated successfully" });

        }
        catch (Exception)
        { 
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }
    
    /// <summary>
    /// Edits the type of a match.
    /// </summary>
    /// <remarks>
    /// This endpoint allows the creator of a match to edit its type.
    /// </remarks>
    /// <param name="matchId">The ID of the match to edit.</param>
    /// <param name="body">The request containing the new match type. Possible values (501, 301, 101)</param>
    /// <returns>The edited match.</returns>
    /// <response code="200">Match type edited successfully.</response>
    /// <response code="400">The request is invalid or the user is not authorized to edit the match.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{matchId}")]
    public async Task<IActionResult> EditMatch(Guid matchId, [FromBody] MatchTypeRequest body)
    {
        try
        {
            if (!_activeMatchService.ActiveMaches.TryGetValue(matchId, out var match))
            {
                return NotFound(new { error = "Match not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !match.isCreator(user.Id))
            {
                return Unauthorized();
            }

            if (match.State != MatchState.Joinable)
            {
                return BadRequest(new { error = "Match already cannot be edited" });
            }

            var validationResult = await _matchTypeValidator.ValidateAsync(body);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            match.Type = body.MatchType;
            return Ok(new {match});
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
        
    }
    
    /// <summary>
    /// Deletes a not started joinable match.
    /// </summary>
    /// <remarks>
    /// This endpoint allows the creator of a match to delete it.
    /// </remarks>
    /// <param name="matchId">The ID of the match to delete.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Match deleted successfully.</response>
    /// <response code="400">The match does not exist or the user is not authorized to delete it.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{matchId}")]
    public async Task<IActionResult> DeleteMatch(Guid matchId)
    {
        try
        {
            if (!_activeMatchService.ActiveMaches.TryGetValue(matchId, out var match))
            {
                return NotFound(new { error = "Match not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !match.isCreator(user.Id))
            {
                return Unauthorized();
            }

            if (match.State != MatchState.Joinable)
            {
                return BadRequest(new { error = "Match already cannot be edited" });
            }

            _activeMatchService.ActiveMaches.Remove(matchId);
            return Ok(new { error = "Match deleted" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
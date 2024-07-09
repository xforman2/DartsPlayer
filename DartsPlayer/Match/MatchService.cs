using DartsPlayer.Data;
using DartsPlayer.Match.Models;
using DartsPlayer.Stats.Models;
using DartsPlayer.User;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DartsPlayer.Match;

public class MatchService
{

    private DataContext _context;
    private UserManager<ApplicationUser> _userManager;
    public MatchService(DataContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ErrorOr<EntityEntry<MatchModel>>> AddMatch(ActiveMatchModel activeMatch)
    {
        ApplicationUser? player1 = await _userManager.FindByIdAsync(activeMatch.Player1.Id.ToString());
        ApplicationUser? player2 =  await _userManager.FindByIdAsync(activeMatch.Player2!.Id.ToString());
        if (player2 == null || player1 == null)
        {
            return Error.Failure("Player not found");
        }
        
        var match = new MatchModel(
              player1.Id, player2.Id, activeMatch.Type, 
             new List<UserMatchStats>()
            {   
                new UserMatchStats
                {
                    PlayerId= player1.Id,
                    NumberOfThrows = activeMatch.Player1.Throws,
                    AverageScore = activeMatch.Player1.TotalScore / (activeMatch.Player1.Throws == 0 ? 1 : 
                        activeMatch.Player1.Throws),
                    DoublesHit = activeMatch.Player1.DoublesHit,
                    AttemptedDoubles = activeMatch.Player1.DoublesTotal,
                    HighestThrow = activeMatch.Player1.HighestScore
                },
                new UserMatchStats
                {
                    PlayerId = player2.Id,
                    NumberOfThrows = activeMatch.Player2.Throws,
                    AverageScore = activeMatch.Player2.TotalScore / (activeMatch.Player2.Throws == 0 ? 1 : 
                        activeMatch.Player2.Throws),
                    DoublesHit =  activeMatch.Player2.DoublesHit,
                    AttemptedDoubles = activeMatch.Player2.DoublesTotal,
                    HighestThrow = activeMatch.Player2.HighestScore
                }}
            );
        
        var result =  await _context.Matches.AddAsync(match); 
        await _context.SaveChangesAsync();
        return result;
        

    }
    
    public async Task<List<MatchModel>> GetAllMatches()
    {
        return await _context.Matches.ToListAsync();
        
    }
}
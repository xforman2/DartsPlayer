using DartsPlayer.Match.Models;
using Microsoft.AspNetCore.Identity;

namespace DartsPlayer.Match;

public class ActiveMatchService
{
    
    public Dictionary<Guid, ActiveMatchModel> ActiveMaches { get; set; }
    public Dictionary<Guid , ActiveMatchModel> PlayedMatches { get; set; }

    public ActiveMatchService()
    {
        ActiveMaches = new Dictionary<Guid, ActiveMatchModel>();
        PlayedMatches = new Dictionary<Guid, ActiveMatchModel>();
    }
    
    public bool PlayerHasMatch(Guid userId)
    {
        foreach (var match in ActiveMaches.Values)
        {
            if (match.Player1.Id == userId || match.Player2?.Id == userId)
            {
                return true;
            }
        }

        foreach (var match in PlayedMatches.Values)
        {
            if (match.Player1.Id == userId || match.Player2?.Id == userId)
            {
                return true;
            }
            
        }

        return false;

    }
    


}
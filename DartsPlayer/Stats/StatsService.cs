using DartsPlayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DartsPlayer.Stats;

public class StatsService
{
    private DataContext _dataContext;

    public StatsService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<dynamic?> GetAllStats(Guid playerId)
    {
        return await _dataContext.UserMatchStats
            .Where(ps => ps.PlayerId == playerId)
            .GroupBy(ps => 1) 
            .Select(g => new
            {
                ThrowCount = g.Sum(ps => ps.NumberOfThrows),
                AverageScore = g.Average(ps => ps.AverageScore),
                DoublesHit = g.Sum(ps => ps.DoublesHit),
                AllDoubles = g.Sum(ps => ps.AttemptedDoubles),
                MaxHighestScore = g.Max(ps => ps.HighestThrow),
                MatchesPlayed = g.Count()
            })
            .FirstOrDefaultAsync();
    }
}
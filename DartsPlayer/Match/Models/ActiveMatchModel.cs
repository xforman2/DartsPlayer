using System.Numerics;
using DartsPlayer.Match.Requests;
using DartsPlayer.Player.Models;
using DartsPlayer.User;
using ErrorOr;

namespace DartsPlayer.Match.Models;

public class ActiveMatchModel
{
    public ActiveMatchModel(PlayerModel player1, MatchType matchType)
    {
        Player1 = player1;
        State = MatchState.Joinable;
        Type = matchType;
        Turn = Player1;
    }
    public PlayerModel Player1 { get; }
    public PlayerModel? Player2 { get; set; }

    public PlayerModel Turn { get; set; }
    public MatchType Type { get; set; }
    public MatchState State { get; set; }
    
    public ErrorOr<ActiveMatchModel> StartMatch(ApplicationUser user)
    {
        if (Player1.Id != user.Id)
        {
            return Error.Failure("You do not have a permission to start this match");
        }
        
        if (Player2 == null)
        {
            return Error.Failure("One player is missing");
        }

        
        
        Player1.ScoreLeft = Convert.ToInt32(Type);
        Player2.ScoreLeft = (int) Type;
        State = MatchState.InProgress;
        Turn = Player1;
        return this;
    }

    public ErrorOr<ActiveMatchModel> JoinMatch(PlayerModel player)
    {
        if (player.Id == Player1.Id)
        {
            return Error.Failure("Player already in the match");
        }

        if (Player2 != null)
        {
            return Error.Failure("Match is already full");
        }

        Player2 = player;
        return this;
    }

    public void UpdateScore(List<Throw> throws)
    {
        var originalScore = Turn.ScoreLeft;
        var throwCount = 0; 
        foreach (var @throw in throws)
        {
            throwCount++;
            var bust = CountThrow(@throw);
            if (bust)
            {
                Turn.ScoreLeft = originalScore;
                throwCount = 3;
                break;
            }
            if (Turn.ScoreLeft == 0)
            {
                break;
            }
            
        }

        var roundScore = originalScore - Turn.ScoreLeft;
        if (roundScore > Turn.HighestScore)
        {
            Turn.HighestScore = roundScore;
        }

        Turn.Throws += throwCount;
        Turn.TotalScore += roundScore;
        if (Turn.ScoreLeft == 0)
        {
            State = MatchState.Ended;
        }
        
        Turn = Turn.Id == Player1.Id ? Player2! : Player1;
    }

    private bool CountThrow(Throw @throw)
    {
        var originalScore = Turn.ScoreLeft;
        var throwScore = @throw.Score * @throw.Multiplier;
        Turn.ScoreLeft -= throwScore;

        if (Turn.ScoreLeft == 0)
        {
            Turn.DoublesTotal++;
            if (@throw.Multiplier == 2)
            {
                Turn.DoublesHit++;
                return false;
            }
            return true;
        }

        if (Turn.ScoreLeft <= 1)
        {
            if (originalScore is <= 40 or 50 && originalScore % 2 == 0)
            {
                Turn.DoublesTotal++;
            }
            return true;
        }

        if (originalScore is <= 40 or 50 && originalScore % 2 == 0)
        {
            Turn.DoublesTotal++;
        }

        return false;



    }

    public bool isCreator(Guid playerId)
    {
        return Player1.Id == playerId;

    }

    public bool isFinished()
    {
        return State == MatchState.Ended;
    }
    
    
}
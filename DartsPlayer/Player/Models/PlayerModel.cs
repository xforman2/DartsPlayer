namespace DartsPlayer.Player.Models;

public class PlayerModel
{
    public Guid Id { get; private set; }
    public int ScoreLeft { get; set; }
    public int Throws { get; set; }

    public int TotalScore { get; set; }
    public int DoublesTotal { get; set; }
    public int DoublesHit { get; set; }

    public int HighestScore { get; set; }


    public PlayerModel(Guid playerId)
    {
        Id = playerId;
        ScoreLeft = 0;
        Throws = 0;
        DoublesTotal = 0;
        DoublesHit = 0;
    }
    
    
    
}
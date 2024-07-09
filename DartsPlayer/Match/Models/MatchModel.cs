using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DartsPlayer.Stats.Models;
using DartsPlayer.User;
using Microsoft.AspNetCore.Identity;

namespace DartsPlayer.Match;

public class MatchModel {
    public MatchModel(Guid player1Id, 
        Guid player2Id, MatchType type, ICollection<UserMatchStats> userMatchStats)
    {
        Player1Id = player1Id;
        Player2Id = player2Id;
        Type = type;
        UserMatchStats = userMatchStats;
    }

    public MatchModel()
    {
        
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public Guid Player1Id { get; set; }
    public ApplicationUser Player1 { get; set; }
    
    public Guid Player2Id { get; set; }
    public ApplicationUser Player2 { get; set; }

    public MatchType Type { get; set; }
    
    public ICollection<UserMatchStats> UserMatchStats { get; set; }
    
}

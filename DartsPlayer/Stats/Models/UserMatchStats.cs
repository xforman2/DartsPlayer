using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DartsPlayer.Match;
using DartsPlayer.User;

namespace DartsPlayer.Stats.Models;

public class 
        UserMatchStats
{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }


        public Guid MatchId { get; set; }
        public MatchModel Match { get; set; }

        
        public Guid PlayerId { get; set; }
        public ApplicationUser Player { get; set; }

        
        public int NumberOfThrows { get; set; }
        
        public int TotalScore { get; }
        public float AverageScore { get; set; }
        public int DoublesHit { get; set; }
        public int AttemptedDoubles { get; set; }
        public int HighestThrow { get; set; }
        
    
}
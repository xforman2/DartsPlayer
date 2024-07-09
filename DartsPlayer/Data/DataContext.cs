using DartsPlayer.Match;
using DartsPlayer.Stats.Models;
using DartsPlayer.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatchType = DartsPlayer.Match.MatchType;

namespace DartsPlayer.Data;

public class DataContext : IdentityDbContext<ApplicationUser , IdentityRole<Guid> , Guid>
{
    public DbSet<MatchModel> Matches { get; set; }
    public DbSet<UserMatchStats> UserMatchStats { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .Entity<MatchModel>()
            .Property(e => e.Type)
            .HasConversion(
                v => v.ToString(),
                v => (MatchType)Enum.Parse(typeof(MatchType), v));
    }
    

}
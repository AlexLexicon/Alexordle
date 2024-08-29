using Alexordle.Client.Application.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Database;
public class AlexordleDbContext : DbContext
{
    public AlexordleDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyScanMarker).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Puzzle> Puzzles { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<AnswerCharacter> AnswerCharacters { get; set; }
    public DbSet<Clue> Clues { get; set; }
    public DbSet<ClueCharacter> ClueCharacters { get; set; }
    public DbSet<Guess> Guesses { get; set; }
    public DbSet<GuessCharacter> GuessCharacters { get; set; }
    public DbSet<Hunch> Hunches { get; set; }
    public DbSet<HunchCharacter> HunchCharacters { get; set; }
}

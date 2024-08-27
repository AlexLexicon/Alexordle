//using Alexordle.Client.Application.Database.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace Alexordle.Client.Application.Database.Entities;
//public class Hint : IEntityTypeConfiguration<Hint>
//{
//    public Guid PuzzleId { get; init; }
//    public RowTypes RowType { get; init; }
//    public int Row { get; init; }
//    public int Column { get; init; }
//    public HintTypes HintType { get; init; }

//    public void Configure(EntityTypeBuilder<Hint> builder)
//    {
//        builder
//            .ToTable("Hint")
//            .HasKey(h => new
//            {
//                h.PuzzleId,
//                h.RowType,
//                h.Row,
//            });

//        builder
//            .Property(h => h.RowType)
//            .HasConversion<string>();

//        builder
//            .Property(h => h.HintType)
//            .HasConversion<string>();
//    }
//}

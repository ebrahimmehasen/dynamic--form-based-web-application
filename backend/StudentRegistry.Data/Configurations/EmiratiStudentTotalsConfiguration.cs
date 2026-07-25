using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class EmiratiStudentTotalsConfiguration : IEntityTypeConfiguration<EmiratiStudentTotals>
    {
        public void Configure(EntityTypeBuilder<EmiratiStudentTotals> builder)
        {
            builder.ToTable("EmiratiStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.FinalTotal).HasPrecision(6, 2);
            builder.Property(t => t.Denominator).HasPrecision(6, 2);
            builder.Property(t => t.Percentage).HasPrecision(5, 2);
            builder.Property(t => t.EquivalentTotal).HasPrecision(6, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.EmiratiTotals)
                .HasForeignKey<EmiratiStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

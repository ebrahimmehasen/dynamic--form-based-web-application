using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class AmericanDiplomaStudentTotalsConfiguration : IEntityTypeConfiguration<AmericanDiplomaStudentTotals>
    {
        public void Configure(EntityTypeBuilder<AmericanDiplomaStudentTotals> builder)
        {
            builder.ToTable("AmericanDiplomaStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.AverageScore).HasPrecision(5, 2);
            builder.Property(t => t.BasePercentage).HasPrecision(5, 2);
            builder.Property(t => t.SatIISubject1).HasMaxLength(50);
            builder.Property(t => t.SatIISubject2).HasMaxLength(50);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.AmericanDiplomaTotals)
                .HasForeignKey<AmericanDiplomaStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

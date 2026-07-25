using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class SaudiStudentTotalsConfiguration : IEntityTypeConfiguration<SaudiStudentTotals>
    {
        public void Configure(EntityTypeBuilder<SaudiStudentTotals> builder)
        {
            builder.ToTable("SaudiStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.YearsCount)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.TotalAchieved)
                .HasPrecision(18, 2);

            builder.Property(t => t.TotalWeighted)
                .HasPrecision(18, 2);

            builder.Property(t => t.SchoolPercentage)
                .HasPrecision(18, 2);

            builder.Property(t => t.AptitudeScore)
                .HasPrecision(18, 2);

            builder.Property(t => t.FinalPercentage)
                .HasPrecision(18, 2);

            builder.Property(t => t.EquivalentTotal)
                .HasPrecision(18, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.SaudiTotals)
                .HasForeignKey<SaudiStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

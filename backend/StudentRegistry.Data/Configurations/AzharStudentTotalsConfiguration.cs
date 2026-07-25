using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class AzharStudentTotalsConfiguration : IEntityTypeConfiguration<AzharStudentTotals>
    {
        public void Configure(EntityTypeBuilder<AzharStudentTotals> builder)
        {
            builder.ToTable("AzharStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.Section).IsRequired().HasMaxLength(20);

            builder.Property(t => t.FinalTotal).HasPrecision(6, 2);
            builder.Property(t => t.Denominator).HasPrecision(6, 2);
            builder.Property(t => t.Percentage).HasPrecision(5, 2);
            builder.Property(t => t.EquivalentTotal).HasPrecision(6, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.AzharTotals)
                .HasForeignKey<AzharStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

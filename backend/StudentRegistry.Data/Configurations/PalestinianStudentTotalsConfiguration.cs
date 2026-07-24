using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class PalestinianStudentTotalsConfiguration : IEntityTypeConfiguration<PalestinianStudentTotals>
    {
        public void Configure(EntityTypeBuilder<PalestinianStudentTotals> builder)
        {
            builder.ToTable("PalestinianStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.Percentage).HasPrecision(5, 2);
            builder.Property(t => t.EquivalentTotal).HasPrecision(7, 2);
            builder.Property(t => t.Branch).IsRequired().HasMaxLength(50);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.PalestinianTotals)
                .HasForeignKey<PalestinianStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class OmaniStudentTotalsConfiguration : IEntityTypeConfiguration<OmaniStudentTotals>
    {
        public void Configure(EntityTypeBuilder<OmaniStudentTotals> builder)
        {
            builder.ToTable("OmaniStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.FinalTotal).HasPrecision(6, 2);
            builder.Property(t => t.Percentage).HasPrecision(5, 2);
            builder.Property(t => t.EquivalentTotal).HasPrecision(6, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.OmaniTotals)
                .HasForeignKey<OmaniStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

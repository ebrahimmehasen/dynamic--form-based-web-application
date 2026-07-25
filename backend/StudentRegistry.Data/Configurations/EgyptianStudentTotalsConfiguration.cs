using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class EgyptianStudentTotalsConfiguration : IEntityTypeConfiguration<EgyptianStudentTotals>
    {
        public void Configure(EntityTypeBuilder<EgyptianStudentTotals> builder)
        {
            builder.ToTable("EgyptianStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.Track).IsRequired().HasMaxLength(50);
            builder.Property(t => t.SubjectSystem).IsRequired().HasMaxLength(20);

            builder.Property(t => t.FinalTotal).HasPrecision(6, 2);
            builder.Property(t => t.Denominator).HasPrecision(6, 2);
            builder.Property(t => t.Percentage).HasPrecision(5, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.EgyptianTotals)
                .HasForeignKey<EgyptianStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class OtherStudentTotalsConfiguration : IEntityTypeConfiguration<OtherStudentTotals>
    {
        public void Configure(EntityTypeBuilder<OtherStudentTotals> builder)
        {
            builder.ToTable("OtherStudentTotals", "dbo");

            builder.HasKey(t => t.StudentId);

            builder.Property(t => t.CertificateName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Percentage).HasPrecision(5, 2);

            builder.HasOne(t => t.Student)
                .WithOne(s => s.OtherTotals)
                .HasForeignKey<OtherStudentTotals>(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

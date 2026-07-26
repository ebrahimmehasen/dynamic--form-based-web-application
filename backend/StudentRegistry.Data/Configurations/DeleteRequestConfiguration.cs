using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class DeleteRequestConfiguration : IEntityTypeConfiguration<DeleteRequest>
    {
        public void Configure(EntityTypeBuilder<DeleteRequest> builder)
        {
            builder.ToTable("DeleteRequests", "dbo");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.RequestedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Editor");

            builder.Property(d => d.RequestedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(d => d.Reason)
                .HasColumnType("nvarchar(max)");

            builder.Property(d => d.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            builder.Property(d => d.ReviewedBy)
                .HasMaxLength(100);

            builder.Property(d => d.ReviewedAt);

            builder.HasIndex(d => d.StudentId);
            builder.HasIndex(d => d.Status);

            // SetNull, not Cascade: approving a request deletes the Student row, and this record must
            // survive that deletion as proof the approval happened (see DeleteRequest.StudentId).
            builder.HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class PendingReviewConfiguration : IEntityTypeConfiguration<PendingReview>
    {
        public void Configure(EntityTypeBuilder<PendingReview> builder)
        {
            builder.ToTable("PendingReviews", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.FlaggedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("User");

            builder.Property(p => p.FlaggedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            builder.Property(p => p.ResolvedBy)
                .HasMaxLength(100);

            builder.Property(p => p.ResolvedAt);

            builder.HasIndex(p => p.StudentId);
            builder.HasIndex(p => p.Status);

            builder.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

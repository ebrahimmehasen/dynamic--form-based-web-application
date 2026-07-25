using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class ReviewNoteConfiguration : IEntityTypeConfiguration<ReviewNote>
    {
        public void Configure(EntityTypeBuilder<ReviewNote> builder)
        {
            builder.ToTable("ReviewNotes", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.FieldName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.FieldValueSnapshot)
                .HasColumnType("nvarchar(max)");

            builder.Property(r => r.ReviewerNote)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(r => r.Author)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("User");

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(r => r.UpdatedAt);

            builder.HasIndex(r => new { r.StudentId, r.FieldName });

            builder.HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

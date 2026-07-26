using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class FieldCommentConfiguration : IEntityTypeConfiguration<FieldComment>
    {
        public void Configure(EntityTypeBuilder<FieldComment> builder)
        {
            builder.ToTable("FieldComments", "dbo");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.FieldName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(f => f.FieldSnapshot)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.CommentText)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.Author)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Editor");

            builder.Property(f => f.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(f => f.UpdatedAt);

            builder.Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("unreviewed");

            builder.HasIndex(f => new { f.StudentId, f.FieldName });
            builder.HasIndex(f => f.Status);

            builder.HasOne(f => f.Student)
                .WithMany()
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

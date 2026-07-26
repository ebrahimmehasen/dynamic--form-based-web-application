using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class FieldEditConfiguration : IEntityTypeConfiguration<FieldEdit>
    {
        public void Configure(EntityTypeBuilder<FieldEdit> builder)
        {
            builder.ToTable("FieldEdits", "dbo");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.FieldName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(f => f.OldValue)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.NewValue)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.Editor)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Editor");

            builder.Property(f => f.EditedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(f => f.Note)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.Source)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("manual");

            builder.HasIndex(f => new { f.StudentId, f.FieldName });

            builder.HasOne(f => f.Student)
                .WithMany()
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction (not Cascade/SetNull): SQL Server rejects a second cascading path from Students
            // to FieldEdits (Students -> FieldComments -> FieldEdits vs. the direct Students ->
            // FieldEdits FK below) as "multiple cascade paths". FieldComments are never row-deleted by
            // app logic anyway (only status-transitioned) — when a Student IS deleted, both FieldEdits
            // and FieldComments are removed together by the same statement via their own direct
            // Students FKs, so this FK is never actually evaluated against a dangling reference.
            builder.HasOne(f => f.SourceComment)
                .WithMany()
                .HasForeignKey(f => f.SourceCommentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

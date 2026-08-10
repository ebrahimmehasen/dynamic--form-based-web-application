using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class CertificationDisplaySettingConfiguration : IEntityTypeConfiguration<CertificationDisplaySetting>
    {
        public void Configure(EntityTypeBuilder<CertificationDisplaySetting> builder)
        {
            builder.ToTable("CertificationDisplaySettings", "dbo");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.CertificationKey)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(s => s.CertificationKey).IsUnique();

            builder.Property(s => s.IsResultVisible)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.UpdatedByUsername)
                .HasMaxLength(50);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students", "dbo");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.StudentName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.StudentNameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.NationalId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(s => s.NationalId)
                .IsUnique();

            builder.Property(s => s.WishCollege)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.WishProgram)
                .HasMaxLength(100);

            builder.Property(s => s.GraduationYear)
                .IsRequired();

            builder.Property(s => s.Gender)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.BirthCountry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.BirthGovernorate)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.BirthCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.BirthDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.SchoolName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.GuardianName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.GuardianNationalId)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.GuardianOccupation)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.GuardianPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.GuardianLandlinePhone)
                .HasMaxLength(20);

            builder.Property(s => s.GuardianRelation)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.AddressGov)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.AddressCenter)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.AddressVillage)
                .HasMaxLength(100);

            builder.Property(s => s.AddressStreet)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.AddressBuilding)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.AddressFloor)
                .HasMaxLength(20);

            builder.Property(s => s.Certification)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Track)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.PhotoPath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(s => s.SubmissionToken)
                .HasMaxLength(64);

            builder.HasIndex(s => s.SubmissionToken)
                .IsUnique()
                .HasFilter("[SubmissionToken] IS NOT NULL");

            builder.Property(s => s.EligibilityStatus)
                .HasMaxLength(20);

            builder.Property(s => s.EligibilityConfirmedBy)
                .HasMaxLength(100);

            builder.Property(s => s.EligibilityNote)
                .HasMaxLength(1000);
        }
    }
}

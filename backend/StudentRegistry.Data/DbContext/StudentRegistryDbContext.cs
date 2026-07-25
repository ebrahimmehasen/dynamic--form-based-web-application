using Microsoft.EntityFrameworkCore;
using StudentRegistry.Domain.Entities;
using System.Reflection;

namespace StudentRegistry.Data.DbContext
{
    public class StudentRegistryDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public StudentRegistryDbContext(DbContextOptions<StudentRegistryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<SaudiStudentTotals> SaudiStudentTotals => Set<SaudiStudentTotals>();
        public DbSet<SaudiStudentGrades> SaudiStudentGrades => Set<SaudiStudentGrades>();
        public DbSet<IgStudentGrades> IgStudentGrades => Set<IgStudentGrades>();
        public DbSet<IgStudentGradeCounts> IgStudentGradeCounts => Set<IgStudentGradeCounts>();
        public DbSet<StandardStudentGrades> StandardStudentGrades => Set<StandardStudentGrades>();
        public DbSet<KuwaitiStudentTotals> KuwaitiStudentTotals => Set<KuwaitiStudentTotals>();
        public DbSet<QatariStudentTotals> QatariStudentTotals => Set<QatariStudentTotals>();
        public DbSet<OmaniStudentTotals> OmaniStudentTotals => Set<OmaniStudentTotals>();
        public DbSet<YemeniStudentTotals> YemeniStudentTotals => Set<YemeniStudentTotals>();
        public DbSet<BahrainiStudentTotals> BahrainiStudentTotals => Set<BahrainiStudentTotals>();
        public DbSet<PalestinianStudentTotals> PalestinianStudentTotals => Set<PalestinianStudentTotals>();
        public DbSet<OtherStudentTotals> OtherStudentTotals => Set<OtherStudentTotals>();
        public DbSet<EgyptianStudentTotals> EgyptianStudentTotals => Set<EgyptianStudentTotals>();
        public DbSet<AzharStudentTotals> AzharStudentTotals => Set<AzharStudentTotals>();
        public DbSet<EmiratiStudentTotals> EmiratiStudentTotals => Set<EmiratiStudentTotals>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}

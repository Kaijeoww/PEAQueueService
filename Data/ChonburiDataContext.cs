using Microsoft.EntityFrameworkCore;
using tutorial.Models;

namespace tutorial.Data
{
    public class ChonburiDataContext : DbContext
    {
        public ChonburiDataContext(DbContextOptions<ChonburiDataContext> options) : base(options)
        {

        }
        public DbSet<CompleteQ> CompleteQ { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompleteQ>().ToTable("CompleteQ");

            modelBuilder.Entity<CompleteQ>()
                .Property(q => q.QPress)
                .IsRequired(); // บังคับให้ QPress ไม่เป็น null

            modelBuilder.Entity<CompleteQ>()
                .Property(q => q.QNumber)
                .IsRequired(); // บังคับให้ QNumber ไม่เป็น
                               // 
            base.OnModelCreating(modelBuilder);
        }
    }
}

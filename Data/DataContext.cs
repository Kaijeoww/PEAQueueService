using Microsoft.EntityFrameworkCore;
using tutorial.Models;

namespace tutorial.Data
{
    public class DataContext : DbContext  
    {
        public DataContext(DbContextOptions<DataContext>options) : base(options) 
        {
            
        }

        public DbSet<CompleteQ> CompleteQ { get; set; }
        public DbSet<Queue_Branch> Queue_Branch { get; set; }
        public DbSet<QueueView> QueueViews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // กำหนดคีย์สำหรับ Queue_Branch (BranchID เป็นคีย์หลัก)
            modelBuilder.Entity<Queue_Branch>()
                .ToTable("Queue_Branch")
                .HasKey(q => q.BranchID);  // กำหนดคีย์หลักเป็น BranchID

            // กำหนดคีย์หลักสำหรับ CompleteQ (ใช้ QNumber เป็นคีย์หลัก)
            modelBuilder.Entity<CompleteQ>()
                .ToTable("CompleteQ")
                .HasKey(c => c.QNumber); // กำหนด QNumber เป็นคีย์หลัก

            // สร้างความสัมพันธ์ระหว่าง CompleteQ และ Queue_Branch
            modelBuilder.Entity<CompleteQ>()
                .HasOne(c => c.Queue_Branch)  // CompleteQ มี Queue_Branch หนึ่งตัว
                .WithMany(b => b.CompleteQs)  // Queue_Branch มี CompleteQ หลายตัว
                .HasForeignKey(c => c.BranchID);  // Foreign Key จาก CompleteQ คือ BranchID

            // ตั้งค่า QueueView ให้ BranchID เป็นคีย์หลัก
            modelBuilder.Entity<QueueView>()
                .ToTable("QueueView")
                .HasKey(q => q.BranchID);  // กำหนดคีย์หลักเป็น BranchID

            base.OnModelCreating(modelBuilder);
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace ItemProcessor.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // These properties map to your SQL Server tables
        public DbSet<User>          Users          { get; set; }
        public DbSet<Item>          Items          { get; set; }
        public DbSet<ProcessedItem> ProcessedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tell EF Core about the two foreign keys from ProcessedItems to Items
            // Without this, EF gets confused about which FK is which
            modelBuilder.Entity<ProcessedItem>()
                .HasOne(p => p.ParentItem)
                .WithMany(i => i.ParentProcessedItems)
                .HasForeignKey(p => p.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete

            modelBuilder.Entity<ProcessedItem>()
                .HasOne(p => p.ChildItem)
                .WithMany(i => i.ChildProcessedItems)
                .HasForeignKey(p => p.ChildItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
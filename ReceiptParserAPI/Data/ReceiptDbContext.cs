using Microsoft.EntityFrameworkCore;
using ReceiptParserAPI.Models;

namespace ReceiptParserAPI.Data
{
    public class ReceiptDbContext : DbContext
    {
        public ReceiptDbContext(DbContextOptions<ReceiptDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bire Çok İlişkiyi Tanımlama: Receipt ve LineItems arasında
            modelBuilder.Entity<LineItem>()
                .HasOne(li => li.Receipt)     // Bir ürün kaleminin bir fişi vardır
                .WithMany(r => r.LineItems)   // Bir fişte birden çok ürün kalemi vardır
                .HasForeignKey(li => li.ReceiptId);

            base.OnModelCreating(modelBuilder);
        }
    }
}

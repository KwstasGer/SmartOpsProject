using Microsoft.EntityFrameworkCore;
using SmartOps.Models;
using SmartOpsProject.Models;

namespace SmartOps.Data
{
    public class SmartOpsDbContext : DbContext
    {
        public SmartOpsDbContext(DbContextOptions<SmartOpsDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        // public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoiceItem> InvoicesItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------- Relations --------------------
            // Customer → Users
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice → Users
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier → Users
            modelBuilder.Entity<Supplier>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → Users
            modelBuilder.Entity<Item>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Service → Users
            modelBuilder.Entity<Service>()
                .HasOne(sv => sv.User)
                .WithMany()
                .HasForeignKey(sv => sv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------- Indexes --------------------
            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.UserId, c.Name });

            modelBuilder.Entity<Supplier>()
                .HasIndex(s => new { s.UserId, s.Name });

            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.UserId, i.Description });

            modelBuilder.Entity<Service>()
                .HasIndex(sv => new { sv.UserId, sv.Description });

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.UserId, i.Series, i.Year, i.Number })
                .IsUnique();

            // -------------------- User entity --------------------
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.Username)
                    .HasMaxLength(160)
                    .IsRequired();

                e.Property(u => u.PasswordHash)
                    .HasMaxLength(256)
                    .IsRequired();

                e.HasIndex(u => u.Username).IsUnique();
            });

            // -------------------- Customer auto code --------------------


            // Υπολογιζόμενος κωδικός: '0' + padding 5 ψηφίων (000001, 000002, ...)
            modelBuilder.Entity<Customer>(e =>
            {
                e.Property(x => x.CustomerCode)
                 .HasColumnType("varchar(16)")
                 .IsRequired(false)
                 .ValueGeneratedOnAdd()
                 .HasDefaultValueSql("('0' + RIGHT('00000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.CustomerCodeSeq), 5))");

                e.HasIndex(x => x.CustomerCode).IsUnique();
            });

        }

    }
}

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
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ρητό table name για ασφάλεια
            modelBuilder.Entity<InvoiceLine>().ToTable("InvoiceLines");

            // -------- Σχέσεις --------
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User).WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer).WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceLine>()
                .HasOne(l => l.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceLine>()
                .HasOne(l => l.Item).WithMany()
                .HasForeignKey(l => l.ItemId)
                .OnDelete(DeleteBehavior.Restrict);


            // -------- Μοναδικός δείκτης --------
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.UserId, i.Series, i.Year, i.Number })
                .IsUnique();

            // -------- Τύποι/precision --------
            modelBuilder.Entity<Invoice>(e =>
            {
                e.Property(p => p.IssueDate).HasColumnType("date");
                e.Property(p => p.TotalNet).HasColumnType("decimal(18,2)");
                e.Property(p => p.TotalVat).HasColumnType("decimal(18,2)");
                e.Property(p => p.TotalGross).HasColumnType("decimal(18,2)");
                e.Property(p => p.PaidAmount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<InvoiceLine>(e =>
            {
                e.Property(p => p.Quantity).HasColumnType("decimal(18,3)");
                e.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
                e.Property(p => p.VatRate).HasColumnType("decimal(6,4)");
                // Αν κρατήσεις Description ως snapshot:
                // e.Property(p => p.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<Customer>(e =>
            {
                e.Property(x => x.CustomerCode)
                 .HasMaxLength(32)
                 .IsRequired()                     // αν θες not null στο schema
                 .ValueGeneratedOnAdd()            // ΠΟΛΥ ΣΗΜΑΝΤΙΚΟ
                 .HasDefaultValueSql("( 'C' + RIGHT('000000' + CAST(NEXT VALUE FOR dbo.CustomerCodeSeq AS varchar(6)), 6) )");

                e.HasIndex(x => new { x.UserId, x.CustomerCode })
                 .IsUnique()
                 .HasDatabaseName("UX_Customers_UserId_CustomerCode");

                e.Property(x => x.Name).HasMaxLength(160).IsRequired();
                e.Property(x => x.VatStatus).HasMaxLength(32).IsRequired();
                e.Property(x => x.CustomerCategory).HasMaxLength(32).IsRequired();
            });

        }
    }
}

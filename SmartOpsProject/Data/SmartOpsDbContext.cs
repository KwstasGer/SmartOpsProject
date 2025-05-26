using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using SmartOps.Models;
using SmartOpsProject.Models;

namespace SmartOps.Data
{
    public class SmartOpsDbContext : DbContext
    {
        public SmartOpsDbContext(DbContextOptions<SmartOpsDbContext> options) : base(options) { }

        public DbSet<Customers> Customers { get; set; }
        public DbSet<Product> Product  { get; set; }
        public DbSet<Service> Services { get; set; }
       // public DbSet<Invoice> Invoices { get; set; }
       // public DbSet<InvoiceLine> InvoiceLines { get; set; }
       // public DbSet<InvoiceItem> InvoicesItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }


    }
}

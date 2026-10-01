using Microsoft.EntityFrameworkCore;
using OemagOps.Models;

namespace OemagOps.Data
{
    public class OemagOpsDbContext : DbContext
    {
        public OemagOpsDbContext(
            DbContextOptions<OemagOpsDbContext> options)
            : base(options)
        {
        }

        public DbSet<IncomingRequest> IncomingRequests { get; set; }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IncomingRequest>()
                .ToTable("OemagOpsIncomingRequests", "dbo");

            modelBuilder.Entity<Product>()
                .ToTable("OemagOpsProducts", "dbo");
        }
    }
}
using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Domain;

namespace PurchaseTransactions.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18,2)");
        });
    }
}

using Microsoft.EntityFrameworkCore;

namespace Dashboard.Models;

public class ZuchiDB : DbContext
{
    public ZuchiDB(DbContextOptions<ZuchiDB> options) : base(options)
    {
    }

    public virtual DbSet<Store> Stores { get; set; }
    public virtual DbSet<Revenue> Revenues { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<TransactionItem> TransactionItems { get; set; }
    public virtual DbSet<Member> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置 Transaction 與 TransactionItem 的關係
        modelBuilder.Entity<TransactionItem>()
            .HasOne(ti => ti.Master)
            .WithMany(t => t.Items)
            .HasForeignKey(ti => ti.MasterID);
    }
}

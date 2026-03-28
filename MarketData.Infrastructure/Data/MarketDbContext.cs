using MarketData.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Infrastructure.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetPrice> AssetPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");
                entity.HasKey(e => e.Symbol);
                entity.Property(e => e.Symbol)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(255);
            });

            modelBuilder.Entity<AssetPrice>(entity =>
            {
                entity.ToTable("AssetPrices");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18, 8)");

                entity.HasOne(d => d.Asset)
                      .WithMany(p => p.Prices)
                      .HasForeignKey(d => d.AssetSymbol)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

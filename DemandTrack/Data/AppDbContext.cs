using DemandTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Demand> Demands => Set<Demand>();
        public DbSet<DemandItem> DemandItems => Set<DemandItem>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
        public DbSet<SupplyUpload> SupplyUploads => Set<SupplyUpload>();
        public DbSet<SupplyItem> SupplyItems => Set<SupplyItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DemandItem relations
            modelBuilder.Entity<DemandItem>()
                .HasOne(di => di.Demand)
                .WithMany(d => d.Items)
                .HasForeignKey(di => di.DemandId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DemandItem>()
                .HasOne(di => di.Book)
                .WithMany()
                .HasForeignKey(di => di.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShipmentItem relations
            modelBuilder.Entity<ShipmentItem>()
                .HasOne(si => si.Shipment)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShipmentItem>()
                .HasOne(si => si.Book)
                .WithMany()
                .HasForeignKey(si => si.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shipment to Demand
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Demand)
                .WithMany()
                .HasForeignKey(s => s.DemandId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyUpload relations
            modelBuilder.Entity<SupplyUpload>()
                .HasOne(su => su.Demand)
                .WithMany()
                .HasForeignKey(su => su.DemandId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupplyItem relations
            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.SupplyUpload)
                .WithMany(su => su.Items)
                .HasForeignKey(si => si.SupplyUploadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.DemandItem)
                .WithMany(di => di.SupplyItems)
                .HasForeignKey(si => si.DemandItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique ISBN
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique();
        }
    }
}

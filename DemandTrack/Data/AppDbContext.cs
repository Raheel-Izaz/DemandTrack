using DemandTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Master Data
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();

        // Demands & Demand Items
        public DbSet<Demand> Demands => Set<Demand>();
        public DbSet<DemandItem> DemandItems => Set<DemandItem>();

        // Supplier Invoices & Items
        public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
        public DbSet<SupplierInvoiceItem> SupplierInvoiceItems => Set<SupplierInvoiceItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category Self-Referencing (Subcategories)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Grade Entity
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Code).HasMaxLength(50);
                entity.HasIndex(g => g.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
            });

            // Product Entity Configurations
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(250);
                entity.Property(p => p.Sku).HasMaxLength(100);
                entity.Property(p => p.Barcode).HasMaxLength(100);
                entity.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(50);
                
                // Decimal Precision (18, 2)
                entity.Property(p => p.DefaultUnitPrice).HasPrecision(18, 2);
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);

                // Unique indexes (Sku and Barcode allowing nulls)
                entity.HasIndex(p => p.Sku).IsUnique().HasFilter("\"Sku\" IS NOT NULL");
                entity.HasIndex(p => p.Barcode).IsUnique().HasFilter("\"Barcode\" IS NOT NULL");

                // PostgreSQL JSONB Attributes column
                entity.Property(p => p.Attributes)
                    .HasColumnType("jsonb");

                // Product -> Category (Restrict)
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Product -> Grade (Nullable FK, Restrict delete)
                entity.HasOne(p => p.Grade)
                    .WithMany(g => g.Products)
                    .HasForeignKey(p => p.GradeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductInventory (1:1 with Product using ProductId as PK and FK)
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.ToTable("ProductInventories");
                entity.HasKey(pi => pi.ProductId);

                entity.HasOne(pi => pi.Product)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<ProductInventory>(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InventoryTransaction (Append-only Ledger)
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("InventoryTransactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ReferenceId).HasMaxLength(100);

                entity.HasOne(t => t.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(t => t.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.ProductId);
                entity.HasIndex(t => t.CreatedAt);
            });

            // Demand relations
            modelBuilder.Entity<Demand>()
                .HasOne(d => d.Supplier)
                .WithMany(s => s.Demands)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.ParentDemand)
                .WithMany(d => d.Backorders)
                .HasForeignKey(d => d.ParentDemandId)
                .OnDelete(DeleteBehavior.Restrict);

            // DemandItem relations
            modelBuilder.Entity<DemandItem>(entity =>
            {
                entity.Property(di => di.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(di => di.Demand)
                    .WithMany(d => d.Items)
                    .HasForeignKey(di => di.DemandId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(di => di.Product)
                    .WithMany(p => p.DemandItems)
                    .HasForeignKey(di => di.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SupplierInvoice relations
            modelBuilder.Entity<SupplierInvoice>(entity =>
            {
                entity.Property(si => si.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(si => si.Demand)
                    .WithMany(d => d.Invoices)
                    .HasForeignKey(si => si.DemandId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(si => si.Supplier)
                    .WithMany(s => s.Invoices)
                    .HasForeignKey(si => si.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SupplierInvoiceItem relations
            modelBuilder.Entity<SupplierInvoiceItem>(entity =>
            {
                entity.Property(sii => sii.UnitPrice).HasPrecision(18, 2);
                entity.Property(sii => sii.LineTotal).HasPrecision(18, 2);
                entity.Property(sii => sii.ConfidenceScore).HasPrecision(5, 4);

                entity.HasOne(sii => sii.SupplierInvoice)
                    .WithMany(si => si.Items)
                    .HasForeignKey(sii => sii.SupplierInvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sii => sii.DemandItem)
                    .WithMany(di => di.InvoiceItems)
                    .HasForeignKey(sii => sii.DemandItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(sii => sii.Product)
                    .WithMany()
                    .HasForeignKey(sii => sii.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

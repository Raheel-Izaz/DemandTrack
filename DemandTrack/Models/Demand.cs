using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Demand
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!; // e.g. DEM-2026-0001
        public string Title { get; set; } = null!;
        public int SeasonId { get; set; }
        public int SupplierId { get; set; }
        public DemandStatus Status { get; set; } = DemandStatus.Draft;
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public Season Season { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
        public ICollection<DemandItem> Items { get; set; } = new List<DemandItem>();
        public ICollection<SupplierInvoice> Invoices { get; set; } = new List<SupplierInvoice>();
    }
}

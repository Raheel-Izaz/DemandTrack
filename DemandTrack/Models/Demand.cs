using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Demand
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!; // e.g. DEM-2026-0001
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "Draft"; // Draft, SentToSupplier, PartiallyFulfilled, Fulfilled, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? TargetDeliveryDate { get; set; }
        public string? Notes { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // Self-referencing relationship for autonomous backorder tracking
        public int? ParentDemandId { get; set; }
        public Demand? ParentDemand { get; set; }
        public ICollection<Demand> Backorders { get; set; } = new List<Demand>();

        public ICollection<DemandItem> Items { get; set; } = new List<DemandItem>();
        public ICollection<SupplierInvoice> Invoices { get; set; } = new List<SupplierInvoice>();
    }
}

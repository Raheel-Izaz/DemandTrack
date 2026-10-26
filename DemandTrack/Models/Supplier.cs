using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Demand> Demands { get; set; } = new List<Demand>();
        public ICollection<SupplierInvoice> Invoices { get; set; } = new List<SupplierInvoice>();
    }
}

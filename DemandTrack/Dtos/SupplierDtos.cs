using System.Collections.Generic;

namespace DemandTrack.Dtos
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public int DemandsCount { get; set; }
        public int InvoicesCount { get; set; }
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace DemandTrack.Dtos
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Contact { get; set; }
        public bool IsActive { get; set; }
        public int DemandsCount { get; set; }
        public int InvoicesCount { get; set; }
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Contact { get; set; }
    }

    public class UpdateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Contact { get; set; }
        public bool IsActive { get; set; }
    }
}

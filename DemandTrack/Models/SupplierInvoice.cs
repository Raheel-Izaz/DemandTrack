using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class SupplierInvoice
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demand? Demand { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public string InvoiceNumber { get; set; } = null!;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public string SourceType { get; set; } = "Excel"; // Excel, PdfDigital, PdfScanned, Image_OCR, Manual
        public string? DocumentFilePath { get; set; }
        public string? RawOcrText { get; set; }
        public decimal TotalAmount { get; set; } = 0.0m;
        public string Status { get; set; } = "Uploaded"; // Uploaded, Parsed, Reconciled, Disputed
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SupplierInvoiceItem> Items { get; set; } = new List<SupplierInvoiceItem>();
    }
}

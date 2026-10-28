using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DemandTrack.Models;

namespace DemandTrack.Dtos
{
    public class DemandListDto
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int SeasonId { get; set; }
        public string SeasonName { get; set; } = null!;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public DemandStatus Status { get; set; }
        public int TotalRequestedQty { get; set; }
        public int TotalSuppliedQty { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class DemandDetailDto
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int SeasonId { get; set; }
        public string SeasonName { get; set; } = null!;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public DemandStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int TotalRequestedQty { get; set; }
        public int TotalSuppliedQty { get; set; }
        public decimal TotalAmount { get; set; }
        public List<DemandItemDto> Items { get; set; } = new();
    }

    public class DemandItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Sku { get; set; }
        public int RequestedQty { get; set; }
        public int SuppliedQty { get; set; }
        public int RemainingQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public DemandItemStatus Status { get; set; }
    }

    public class CreateDemandRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public int SeasonId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        public string? Notes { get; set; }

        public DemandStatus Status { get; set; } = DemandStatus.Draft;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<CreateDemandItemRequest> Items { get; set; } = new();
    }

    public class CreateDemandItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "RequestedQty must be greater than 0.")]
        public int RequestedQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be non-negative.")]
        public decimal UnitPrice { get; set; }
    }

    public class UpdateDemandRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public int SupplierId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<CreateDemandItemRequest> Items { get; set; } = new();
    }

    public class UpdateDemandStatusRequest
    {
        [Required]
        public DemandStatus Status { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
    }
}

using System.Collections.Generic;

namespace DemandTrack.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int SubCategoriesCount { get; set; }
        public int ItemsCount { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace OemagOps.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int? PowerWp { get; set; }

        public string? Color { get; set; }

        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; }
    }
}
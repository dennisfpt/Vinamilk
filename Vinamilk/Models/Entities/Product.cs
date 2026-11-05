namespace Vinamilk.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public string ShortDescription { get; set; } = string.Empty;
        public string NutritionInfo { get; set; } = string.Empty;
        public int StockQuantity { get; set; } = 100;
        public bool IsFeatured { get; set; } = false;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
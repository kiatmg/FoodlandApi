namespace FoodLand.Models
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public double? DiscountPercentage { get; set; } 
        public decimal PriceAfterDiscount { get; set; } = 0;
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

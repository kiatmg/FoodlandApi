using System.ComponentModel.DataAnnotations;

namespace FoodLand.Models
{
    public class Cart
    {
        
        public int Id { get; set; }

        
        public string UserId { get; set; }

        
        public int FoodId { get; set; }

        public int Quantity { get; set; }
        public Food Food { get; set; }
    }
}

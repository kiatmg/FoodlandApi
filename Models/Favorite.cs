using System.ComponentModel.DataAnnotations;

namespace FoodLand.Models
{
    public class Favorite
    {
        
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FoodId { get; set; }
        public Food Food { get; set; }
    }
}

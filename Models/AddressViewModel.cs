using System.ComponentModel.DataAnnotations;

namespace FoodLand.Models
{
    public class AddressViewModel
    {
        
        [MaxLength(95)]
        public string StreetAddress { get; set; }
    }
}

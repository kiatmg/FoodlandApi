using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodLand.Models
{
    public class Address
    {
       
        public int Id { get; set; }

        
        [MaxLength(95)]
        public string StreetAddress { get; set; }

        
        public string UserId { get; set; }

       
        public IdentityUser User { get; set; }
    }
}

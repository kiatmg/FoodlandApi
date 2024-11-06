﻿namespace FoodLand.Models
{
   public class OrderItem
{
        public int Id { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}


using FoodLand.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FoodLand.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

           
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Pizza" },
                new Category { Id = 2, Name = "Burger" },
                new Category { Id = 3, Name = "Salad" },
                new Category { Id = 4, Name = "Drink" }
            );

           
            builder.Entity<Food>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Foods)
                .HasForeignKey(f => f.CategoryId);

            builder.Entity<Favorite>()
            .HasOne(f => f.Food)
            .WithMany()
            .HasForeignKey(f => f.FoodId);

            builder.Entity<Cart>()
           .HasOne(f => f.Food)
           .WithMany()
           .HasForeignKey(f => f.FoodId);

            builder.Entity<Address>()
        .HasOne(a => a.User)
        .WithMany()
        .HasForeignKey(a => a.UserId)
        .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
        .Property<string>("UserId");

            builder.Entity<Cart>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
        .HasMany(o => o.OrderItems)
        .WithOne()
        .HasForeignKey(oi => oi.OrderId);
        }

    }
}

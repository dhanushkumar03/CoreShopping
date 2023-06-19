using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core_Web_API.Models.ProductModels;
namespace Core_Web_API.Models
{
    public class ApplicationContext : IdentityDbContext<UserModel>
    {
        public ApplicationContext(DbContextOptions options)
    : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartReference> Carts { get; set; }
        public DbSet<PlacedOrders> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartReference>()
          .HasKey(m => new { m.CartDRefId, m.ProductRefId });
            modelBuilder.Entity<PlacedOrders>()
          .HasKey(m => new { m.CartDRefId, m.ProductRefId, m.OrderNumber });
            base.OnModelCreating(modelBuilder);
            this.SeedUsers(modelBuilder);
            this.SeedRoles(modelBuilder);
            this.SeedUserRoles(modelBuilder);
        }
        private void SeedUsers(ModelBuilder builder)
        {
            UserModel user1 = new UserModel()
            {
                Id = "b74ddd14-6340-4840-95c2-db12554843e5",
                UserName = "Admin",
                NormalizedUserName = "Admin",
                Email = "admin@gmail.com",
                NormalizedEmail = "admin@gmail.com",
            };
            UserModel user2 = new UserModel()
            {
                Id = "b74ddd14-6340-4840-95c2-db12554843e6",
                UserName = "User",
                NormalizedUserName = "User",
                Email = "user@gmail.com",
                NormalizedEmail = "user@gmail.com"
            };

            PasswordHasher<UserModel> passwordHasher1 = new PasswordHasher<UserModel>();
            user1.PasswordHash = passwordHasher1.HashPassword(user1, "admin@123");

            PasswordHasher<UserModel> passwordHasher2 = new PasswordHasher<UserModel>();
            user2.PasswordHash = passwordHasher2.HashPassword(user2, "user@123");

            
            builder.Entity<UserModel>().HasData(user1);
            builder.Entity<UserModel>().HasData(user2);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895711", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Id = "c7b013f0-5201-4317-abd8-c211f91b7330", Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
                );
        }
        private void SeedUserRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>() { RoleId = "fab4fac1-c546-41de-aebc-a14da6895711", UserId = "b74ddd14-6340-4840-95c2-db12554843e5" }
                );
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>() { RoleId = "c7b013f0-5201-4317-abd8-c211f91b7330", UserId = "b74ddd14-6340-4840-95c2-db12554843e6" }
                );
        }
    }
}

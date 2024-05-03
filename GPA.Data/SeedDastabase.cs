using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data
{
    public static class SeedDastabase
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductLocation>().HasData(
                    new ProductLocation { Id = Guid.NewGuid(), Code = "ST-1", Name = "Estante 1", Description = "Estante 1" },
                    new ProductLocation { Id = Guid.NewGuid(), Code = "ST-2", Name = "Estante 2", Description = "Estante 2" }
                );

            Guid cat1 = Guid.NewGuid();
            Guid cat2 = Guid.NewGuid();
            modelBuilder.Entity<Category>().HasData(
                 new Category
                 {
                     Id = cat1,
                     Name = "Botellita",
                     Description = "Botellitas pequeñas"
                 },
                 new Category
                 {
                     Id = cat2,
                     Name = "Botellon",
                     Description = "Botellones de los grandes",
                 }
             );

            var passwordHasher = new PasswordHasher<GPAUser>();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new GPAUser
            {
                Id = userId,
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com"
            };

            var role = new GPARole
            {
                Id = roleId,
                Name = "admin",
                NormalizedName = "ADMIN",
            };

            var roleClaims = new GPARoleClaim[]
            {
                new GPARoleClaim
                {
                    Id = 1,
                    RoleId = roleId,
                    ClaimType = "category",
                    ClaimValue = "c,r,u,d",
                }
            };

            var userRoles = new GPAUserRole[]
            {
                new GPAUserRole
                {
                    Id= Guid.NewGuid(),
                    UserId = userId,
                    RoleId = roleId,
                }
            };
            user.PasswordHash = passwordHasher.HashPassword(user, "admin");
            modelBuilder.Entity<GPAUser>().HasData(user);
            modelBuilder.Entity<GPARole>().HasData(role);
            modelBuilder.Entity<GPAUserRole>().HasData(userRoles);
            modelBuilder.Entity<GPARoleClaim>().HasData(roleClaims);

            var reasons = new Reason[] 
            {
                new Reason { Id = 1, Name = "Purchase" , Description = "Purchase" },
                new Reason { Id = 2, Name = "Sale" , Description = "Sale" },
                new Reason { Id = 3, Name = "Return" , Description = "Return" },
                new Reason { Id = 4, Name = "Adjustment" , Description = "Adjustment" },
                new Reason { Id = 5, Name = "Manufactured" , Description = "Adjustment" },
            };

            modelBuilder.Entity<Reason>().HasData(reasons);
        }
    }
}

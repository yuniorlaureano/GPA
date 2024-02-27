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

            // Guid product1 = Guid.NewGuid();
            // Guid product2 = Guid.NewGuid();
            // modelBuilder.Entity<Product>().HasData(
            //      new Product
            //      {
            //          Id = product1,
            //          Code = "Prod-1",
            //          Name = "Botellon",
            //          UnitCost = 23,
            //          WholesaleCost = 23,
            //          CategoryId = cat1
            //      },
            //      new Product
            //      {
            //          Id = product2,
            //          Code = "Prod-2",
            //          Name = "Botellita",
            //          UnitCost = 23,
            //          WholesaleCost = 23,
            //          CategoryId = cat2
            //      }
            //);

            // Guid provider1 = Guid.NewGuid();
            // Guid provider2 = Guid.NewGuid();
            // modelBuilder.Entity<Provider>().HasData(
            //         new Provider
            //         {
            //             Id = provider1,
            //             Name = "Botellones FT",
            //             RNC = "R898585",
            //             Phone = "48757874",
            //             Email = "ft@botellones.com",
            //         },
            //         new Provider
            //         {
            //             Id = provider2,
            //             Name = "Botellones ST",
            //             RNC = "R898585",
            //             Phone = "48757874",
            //             Email = "ST@botellones.com"
            //         }
            //     );

            // Guid address1 = Guid.NewGuid();
            // modelBuilder.Entity<ProviderAddress>().HasData(
            //         new ProviderAddress
            //         {
            //             Id = address1,
            //             Street = "Juan Pablo Duarte",
            //             BuildingNumber = "34-B",
            //             City = "Santo Domingo",
            //             State = "Santo Domingo Este",
            //             PostalCode = "4545",
            //             ProviderId = provider1
            //         }
            //     );

            // modelBuilder.Entity<Stock>().HasData(
            //      new Stock
            //      {
            //          Id = Guid.NewGuid(),
            //          Code = "INV-1",
            //          Input = 100,
            //          OutInput = 0,
            //          ProductId = product1,
            //          ProviderId = provider1
            //      },
            //      new Stock
            //      {
            //          Id = Guid.NewGuid(),
            //          Code = "INV-2",
            //          Input = 10,
            //          OutInput = 50,
            //          ProductId = product1,
            //          ProviderId = provider1
            //      },
            //      new Stock
            //      {
            //          Id = Guid.NewGuid(),
            //          Code = "INV-3",
            //          Input = 0,
            //          OutInput = 20,
            //          ProductId = product1,
            //          ProviderId = provider1
            //      },
            //      new Stock
            //      {
            //          Id = Guid.NewGuid(),
            //          Code = "INV-4",
            //          Input = 100,
            //          OutInput = 0,
            //          ProductId = product2,
            //          ProviderId = provider2
            //      },
            //      new Stock
            //      {
            //          Id = Guid.NewGuid(),
            //          Code = "INV-5",
            //          Input = 0,
            //          OutInput = 50,
            //          ProductId = product2,
            //          ProviderId = provider2
            //      }
            //    );
        }
    }
}

﻿using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Security;
using GPA.Entities.General;
using GPA.Utils;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

            Guid cat1 = GuidHelper.NewSequentialGuid();
            Guid cat2 = GuidHelper.NewSequentialGuid();
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
            var userId = GuidHelper.NewSequentialGuid();
            var roleId = GuidHelper.NewSequentialGuid();
            var user = new GPAUser
            {
                Id = userId,
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "admin@gmail.com".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
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
                new Reason { Id = 1, NormalizedName = "Purchase", Name = "Compra" , Description = "Compra", TransactionType = TransactionType.Input },
                new Reason { Id = 2, NormalizedName = "Sale", Name = "Venta" , Description = "Venta", TransactionType = TransactionType.Output },
                new Reason { Id = 3, NormalizedName = "Return", Name = "Devolución" , Description = "Devolución", TransactionType = TransactionType.Input },
                new Reason { Id = 4, NormalizedName = "Manufactured", Name = "Manufacturado" , Description = "Manufacturado", TransactionType = TransactionType.Input },
                new Reason { Id = 5, NormalizedName = "DamagedProduct", Name = "Producto defectuoso" , Description = "Producto defectuoso", TransactionType = TransactionType.Output },
                new Reason { Id = 6, NormalizedName = "ExpiredProduct", Name = "Producto expirado" , Description = "Producto expirado", TransactionType = TransactionType.Output },
                new Reason { Id = 7, NormalizedName = "RawMaterial", Name = "Materia prima" , Description = "Materia prima", TransactionType = TransactionType.Output },
            };

            modelBuilder.Entity<Reason>().HasData(reasons);

            var adminProfileId = GuidHelper.NewSequentialGuid();
            modelBuilder.Entity<GPAProfile>().HasData(
                new GPAProfile
                {
                    Id = adminProfileId,
                    Name = "administrador",
                    Value = JsonSerializer.Serialize(ProfileConstants.MasterProfile, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                },
                new GPAProfile
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Name = "cajero"
                },
                new GPAProfile
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Name = "genrente"
                },
                new GPAProfile
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Name = "delivery"
                },
                new GPAProfile
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Name = "gestor de planta"
                }
            );

            modelBuilder.Entity<GPAUserProfile>().HasData(
                new GPAUserProfile
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    UserId = userId,
                    ProfileId = adminProfileId,                    
                }
            );

        }
    }
}

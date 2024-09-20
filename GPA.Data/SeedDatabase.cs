using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Security;
using GPA.Entities.General;
using GPA.Entities.Report;
using GPA.Utils;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data
{
    public static class SeedDatabase
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                 new Category
                 {
                     Id = GuidHelper.NewSequentialGuid(),
                     Name = "Botellita",
                     Description = "Botellitas pequeñas"
                 },
                 new Category
                 {
                     Id = GuidHelper.NewSequentialGuid(),
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
                EmailConfirmed = true,
                Invited = true,
                Deleted = false,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            user.PasswordHash = passwordHasher.HashPassword(user, "admin");
            modelBuilder.Entity<GPAUser>().HasData(user);

            var reasons = new Reason[]
            {
                new Reason { Id = 1, NormalizedName = "Purchase", Name = "Compra" , Description = "Compra", TransactionType = TransactionType.Input },
                new Reason { Id = 2, NormalizedName = "Sale", Name = "Venta" , Description = "Venta", TransactionType = TransactionType.Output },
                new Reason { Id = 3, NormalizedName = "Return", Name = "Devolución" , Description = "Devolución", TransactionType = TransactionType.Input },
                new Reason { Id = 4, NormalizedName = "Manufactured", Name = "Manufacturado" , Description = "Manufacturado", TransactionType = TransactionType.Input },
                new Reason { Id = 5, NormalizedName = "DamagedProduct", Name = "Producto defectuoso" , Description = "Producto defectuoso", TransactionType = TransactionType.Output },
                new Reason { Id = 6, NormalizedName = "ExpiredProduct", Name = "Producto expirado" , Description = "Producto expirado", TransactionType = TransactionType.Output },
                new Reason { Id = 7, NormalizedName = "RawMaterial", Name = "Materia prima" , Description = "Materia prima", TransactionType = TransactionType.Output },
                new Reason { Id = 8, NormalizedName = "CanceledOutput", Name = "Salida cancelada" , Description = "Salida cancelada", TransactionType = TransactionType.Input },
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

            modelBuilder.Entity<ReportTemplate>().HasData(
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.STOCK_CYCLE_DETAILS_TEMPLATE,
                    Template = TemplateConstants.StockDetailsTemplate()
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.TRANSACTION_TEMPLATE,
                    Template = TemplateConstants.TransactionTemplate()
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.SALE_TEMPLATE,
                    Template = TemplateConstants.SaleTemplate()
                },

                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.INVOICE_TEMPLATE,
                    Template = TemplateConstants.InvoiceTemplate(),
                    Width = 65,
                    Height = 200
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.PROOF_OF_PAYMENT_TEMPLATE,
                    Template = TemplateConstants.ProofOfPaymentTemplate(),
                    Width = 65,
                    Height = 200
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE,
                    Template = TemplateConstants.ReceivableProofOfPaymentTemplate(),
                    Width = 65,
                    Height = 200
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.EXISTENCE_TEMPLATE,
                    Template = TemplateConstants.ExistenceTemplate()
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.USER_INVITATION_TEMPLATE,
                    Template = TemplateConstants.GetUserInvitationEmailTemplate()
                },
                new ReportTemplate
                {
                    Id = GuidHelper.NewSequentialGuid(),
                    Code = TemplateConstants.PASSWORD_RESET_TEMPLATE,
                    Template = TemplateConstants.GetPasswordResetTemplate()
                }
            );
        }
    }
}

using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Dtos.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<RawProduct?> GetProductAsync(Guid id);
        Task<IEnumerable<RawProduct>> GetProductsAsync(RequestFilterDto filter);
        Task<int> GetProductsCountAsync(RequestFilterDto filter);
        Task<IEnumerable<RawProduct>> GetProductsAsync(List<Guid> ids);
        Task SavePhoto(string fullFileName, Guid productId);
        Task SoftDelete(Guid productId);
        Task AddHistory(RawProduct product, string action, Guid by);
        Task<IEnumerable<RawRelatedProductRead>> GetRelatedProductsByProductIdAsync(Guid id);
        Task DeleteRelatedProductsByProductIdAsync(Guid productId);
        Task<IEnumerable<RawRelatedProduct>> GetRawRelatedProductsByProductIdAsync(List<Guid> ids);
    }

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawProduct?> GetProductAsync(Guid id)
        {
            var query = @"
                SELECT 
                     PRO.[Id]
                    ,PRO.[Code]
                    ,PRO.[Name]
                    ,PRO.[Photo]
                    ,PRO.[Price]
                    ,PRO.[Description]
                    ,PRO.[Type]
                    ,PRO.[UnitValue]
                    ,PRO.[UnitId]
                    ,PRO.[CategoryId]
                    ,PRO.[ProductLocationId]
	                ,UN.[Name] Unit
	                ,CA.[Name] Category
	                ,PL.[Name] ProductLocation
                FROM [GPA].[Inventory].[Products] PRO
                    JOIN [GPA].[General].[Units] UN
                        ON PRO.UnitId = UN.Id AND UN.Deleted = 0
                    JOIN [GPA].[Inventory].[Categories] CA
                        ON PRO.CategoryId = CA.Id AND CA.Deleted = 0
                    LEFT JOIN [GPA].[Inventory].[ProductLocations] PL
                        ON PL.Id = PRO.ProductLocationId AND PL.Deleted = 0
                WHERE PRO.Id = @Id AND PRO.Deleted = 0
            ";

            return await _context.Database.SqlQueryRaw<RawProduct>(
                query,
                new SqlParameter("@Id", id)
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProduct>> GetProductsAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 PRO.[Id]
                    ,PRO.[Code]
                    ,PRO.[Name]
                    ,PRO.[Photo]
                    ,PRO.[Price]
                    ,PRO.[Description]
                    ,PRO.[Type]
                    ,PRO.[UnitValue]
                    ,PRO.[UnitId]
                    ,PRO.[CategoryId]
                    ,PRO.[ProductLocationId]
	                ,UN.[Name] Unit
	                ,CA.[Name] Category
	                ,PL.[Name] ProductLocation
                FROM [GPA].[Inventory].[Products] PRO
	                JOIN [GPA].[General].[Units] UN
		                ON PRO.UnitId = UN.Id AND UN.Deleted = 0
	                JOIN [GPA].[Inventory].[Categories] CA
		                ON PRO.CategoryId = CA.Id AND CA.Deleted = 0
	                LEFT JOIN [GPA].[Inventory].[ProductLocations] PL
		                ON PL.Id = PRO.ProductLocationId AND PL.Deleted = 0
                WHERE 
                  PRO.Deleted = 0 AND (
	              @Search IS NULL
	              OR PRO.[Code] LIKE CONCAT('%', @Search, '%')
	              OR PRO.[Name] LIKE CONCAT('%', @Search, '%')
	              OR CA.[Name] LIKE CONCAT('%', @Search, '%'))
                ORDER BY PRO.Id DESC
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawProduct>(query, Page, PageSize, Search).ToListAsync();
        }


        public async Task<int> GetProductsCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Products] PRO
                    JOIN [GPA].[Inventory].[Categories] CA
		                ON PRO.CategoryId = CA.Id AND CA.Deleted = 0
                WHERE PRO.Deleted = 0 AND (
	                @Search IS NULL
	                OR PRO.[Code] LIKE CONCAT('%', @Search, '%')
	                OR PRO.[Name] LIKE CONCAT('%', @Search, '%')
                    OR CA.[Name] LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProduct>> GetProductsAsync(List<Guid> ids)
        {
            var query = @$"
                SELECT 
                     PRO.[Id]
                    ,PRO.[Code]
                    ,PRO.[Name]
                    ,PRO.[Photo]
                    ,PRO.[Price]
                    ,PRO.[Description]
                    ,PRO.[Type]
                    ,PRO.[UnitValue]
                    ,PRO.[UnitId]
                    ,PRO.[CategoryId]
                    ,PRO.[ProductLocationId]
	                ,UN.[Name] Unit
	                ,CA.[Name] Category
	                ,PL.[Name] ProductLocation
                FROM [GPA].[Inventory].[Products] PRO
                    JOIN [GPA].[General].[Units] UN
                        ON PRO.UnitId = UN.Id AND UN.Deleted = 0
                    JOIN [GPA].[Inventory].[Categories] CA
                        ON PRO.CategoryId = CA.Id AND CA.Deleted = 0
                    LEFT JOIN [GPA].[Inventory].[ProductLocations] PL
                        ON PL.Id = PRO.ProductLocationId AND PL.Deleted = 0
                WHERE PRO.Id IN({string.Join(",", ids.Select(id => $"'{id}'"))}) AND PRO.Deleted = 0
            ";

            return await _context.Database.SqlQueryRaw<RawProduct>(query).ToListAsync();
        }

        public async Task SavePhoto(string fullFileName, Guid productId)
        {
            var query = @$"
                UPDATE [GPA].[Inventory].[Products]
                SET Photo = @Photo 
                WHERE Id = @Id 
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter("@Photo", fullFileName), new SqlParameter("@Id", productId));
        }

        public async Task SoftDelete(Guid productId)
        {
            await _context.Products.Where(x => x.Id == productId).ExecuteUpdateAsync(
                x => x.SetProperty(p => p.Deleted, true)
                      .SetProperty(p => p.DeletedAt, DateTimeOffset.UtcNow)
                );
        }

        public async Task AddHistory(RawProduct product, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[ProductHistory]
                       ([Code]
                       ,[Name]
                       ,[Photo]
                       ,[Price]
                       ,[Description]
                       ,[Type]
                       ,[Unit]
                       ,[Category]
                       ,[ProductLocation]
                       ,[IdentityId]
                       ,[Action]
                       ,[By]
                       ,[At])
                 VALUES
                       (@Code
                       ,@Name
                       ,@Photo
                       ,@Price
                       ,@Description
                       ,@Type
                       ,@Unit
                       ,@Category
                       ,@ProductLocation
                       ,@IdentityId
                       ,@Action
                       ,@By
                       ,@At)
                    ";

            var parameters = new SqlParameter[]
            {
                new("@Code", product.Code)
               ,new("@Name", product.Name)
               ,new("@Photo", product.Photo)
               ,new("@Price", product.Price)
               ,new("@Description", product.Description)
               ,new("@Type", product.Type)
               ,new("@Unit", product.Unit)
               ,new("@Category", product.Category)
               ,new("@ProductLocation", product.ProductLocation ?? "")
               ,new("@IdentityId", product.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }

        public async Task<IEnumerable<RawRelatedProductRead>> GetRelatedProductsByProductIdAsync(Guid id)
        {
            var query = @"
                SELECT 
	                 RP.[Id]
                    ,RP.[ProductId]
                    ,RP.[RelatedProductId]
                    ,RP.[Quantity]
	                ,P.Code
	                ,P.[Name]
	                ,P.Photo
                FROM [gpa].[Inventory].[RelatedProducts] RP
	                JOIN gpa.Inventory.Products P ON RP.[RelatedProductId] = P.Id
                WHERE 
	                RP.[ProductId] = @ProductID
            ";

            return await _context.Database.SqlQueryRaw<RawRelatedProductRead>(
                query,
                new SqlParameter("@ProductID", id)
            ).ToListAsync();
        }

        public async Task<IEnumerable<RawRelatedProduct>> GetRawRelatedProductsByProductIdAsync(List<Guid> ids)
        {
            var query = @$"
                SELECT 
	                 [Id]
                    ,[ProductId]
                    ,[RelatedProductId]
                    ,[Quantity]
                FROM [gpa].[Inventory].[RelatedProducts]
                WHERE [ProductId] IN({string.Join(",", ids.Select(id => $"'{id}'"))})
                            ";

            return await _context.Database.SqlQueryRaw<RawRelatedProduct>(query).ToListAsync();
        }

        public async Task DeleteRelatedProductsByProductIdAsync(Guid productId)
        {
            await _context.RelatedProducts.Where(x => x.ProductId == productId).ExecuteDeleteAsync();
        }
    }
}

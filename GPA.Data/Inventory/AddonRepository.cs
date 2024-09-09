using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Dtos.Audit;
using GPA.Dtos.Inventory;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace GPA.Data.Inventory
{
    public interface IAddonRepository : IRepository<Addon>
    {
        Task<RawAddonsList?> GetAddonsAsync(Guid id);
        Task<IEnumerable<RawAddonsList>> GetAddonsAsync(RequestFilterDto filter);
        Task<int> GetAddonsCountAsync(RequestFilterDto filter);
        Task<List<Addon>> GetAddonsByProductId(Guid productId);
        Task<List<RawAddons>> GetAddonsByProductId(List<Guid> productIds);
        Task<Dictionary<Guid, List<RawAddons>>> GetAddonsByProductIdAsDictionary(List<Guid> productIds);
        Task DeleteAddonsByProductId(Guid productId);
        Task SoftDeleteAddonAsync(Guid id);
        Task<int> GetProductsCountByAddonIdAsync(Guid addonId, RequestFilterDto filter);
        Task<IEnumerable<RawProductByAddonId>> GetProductsByAddonIdAsync(Guid addonId, RequestFilterDto filter);
        Task RemoveAddonFromProductAsync(Guid addonId, Guid productId, Guid createdBy);
        Task AssignAddonToProductAsync(Guid addonId, Guid productId, Guid createdBy);
        Task RemoveAddonFromAllProductAsync(Guid addonId, Guid createdBy);
        Task AssignAddonToAllProductAsync(Guid addonId, Guid createdBy);
        Task AddHistory(Addon addon, string action, Guid by);
    }

    public class AddonRepository : Repository<Addon>, IAddonRepository
    {
        public AddonRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<List<Addon>> GetAddonsByProductId(Guid productId)
        {
            return await _context
                 .Addons
                 .FromSqlRaw(
                     @"SELECT * 
                      FROM [GPA].[Inventory].[Addons] 
                      WHERE Id IN (
                            SELECT AddonId 
                            FROM [GPA].[Inventory].[ProductAddons]
                            WHERE ProductId = {0}
                        )", productId)
                 .ToListAsync();
        }

        public async Task<List<RawAddons>> GetAddonsByProductId(List<Guid> productIds)
        {
            if (productIds is { Count: 0 })
            {
                return new List<RawAddons>();
            }
#pragma warning disable EF1002 // Possible SQL injection vulnerability.
            return await _context.Database.SqlQueryRaw<RawAddons>
                    (
                     @$"
                        SELECT 
	                         AD.[Id]
                            ,AD.[Concept]
                            ,AD.[IsDiscount]
                            ,AD.[Type]
                            ,AD.[Value]
	                        ,PAD.ProductId
                        FROM [GPA].[Inventory].[Addons] AD
	                        JOIN [GPA].[Inventory].[ProductAddons] PAD ON PAD.AddonId = AD.Id
                            WHERE PAD.ProductId in ({string.Join(",", productIds.Select(id => $"'{id}'"))})
                    ").ToListAsync();
        }

        public async Task<Dictionary<Guid, List<RawAddons>>> GetAddonsByProductIdAsDictionary(List<Guid> productIds)
        {
#pragma warning disable EF1002 // Possible SQL injection vulnerability.

            var addons = await GetAddonsByProductId(productIds);
            Dictionary<Guid, List<RawAddons>> mappedAddons = new();
            foreach (var addon in addons)
            {
                if (!mappedAddons.ContainsKey(addon.ProductId))
                {
                    mappedAddons.Add(addon.ProductId, new List<RawAddons>());
                }

                mappedAddons[addon.ProductId].Add(new RawAddons
                {
                    Id = addon.Id,
                    Concept = addon.Concept,
                    IsDiscount = addon.IsDiscount,
                    Type = addon.Type,
                    Value = addon.Value
                });
            }
            return mappedAddons;
        }

        public async Task DeleteAddonsByProductId(Guid productId)
        {
            await _context
                 .ProductAddons
                 .Where(x => x.ProductId == productId)
                 .ExecuteDeleteAsync();
        }

        public async Task<RawAddonsList?> GetAddonsAsync(Guid id)
        {
            var query = @"
              SELECT [Id]
                ,[Concept]
                ,[IsDiscount]
                ,[Type]
                ,[Value]
              FROM [GPA].[Inventory].[Addons]
              WHERE Id = @Id
                    ";

            return await _context.Database
                .SqlQueryRaw<RawAddonsList>(query, new SqlParameter("@Id", id))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawAddonsList>> GetAddonsAsync(RequestFilterDto filter)
        {
            var (addonFilterDto, conceptFilter, isDiscountFilter, typeFilter) = SetFilterParametersIfNotEmpty(filter);

            var query = @$"
              SELECT [Id]
                ,[Concept]
                ,[IsDiscount]
                ,[Type]
                ,[Value]
              FROM [GPA].[Inventory].[Addons]
                WHERE 1 = 1
                    {conceptFilter}
                    {isDiscountFilter}
                    {typeFilter}
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
                    ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);
            AddFilterParameters(addonFilterDto, conceptFilter, isDiscountFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<RawAddonsList>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetAddonsCountAsync(RequestFilterDto filter)
        {
            var (addonFilterDto, conceptFilter, isDiscountFilter, typeFilter) = SetFilterParametersIfNotEmpty(filter);

            var query = @$"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Addons]
                WHERE 1 = 1 
                    {conceptFilter}
                    {isDiscountFilter}
                    {typeFilter}
                    ";

            var parameters = new List<SqlParameter>();
            AddFilterParameters(addonFilterDto, conceptFilter, isDiscountFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        private (AddonFilterDto? addonFilterDto, string conceptFilter, string isDiscountFilter, string typeFilter) SetFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var addonFilterDto = new AddonFilterDto();
            if (filter.Search is { Length: > 0 })
            {
                addonFilterDto = JsonSerializer.Deserialize<AddonFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var conceptFilter = addonFilterDto?.Concept is { Length: > 0 } ? "AND Concept LIKE CONCAT('%', @Concept, '%')" : "";
            var isDiscountFilter = addonFilterDto?.IsDiscount is { Length: > 0 } ? "AND IsDiscount = @IsDiscount" : "";
            var typeFilter = addonFilterDto?.Type is { Length: > 0 } ? "AND [Type] = @Type" : "";

            return (addonFilterDto, conceptFilter, isDiscountFilter, typeFilter);
        }

        private void AddFilterParameters(AddonFilterDto? addonFilterDto, string conceptFilter, string isDiscountFilter, string typeFilter, List<SqlParameter> parameters)
        {
            if (conceptFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Concept", addonFilterDto?.Concept));
            }

            if (isDiscountFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@IsDiscount", bool.Parse(addonFilterDto?.IsDiscount!)));
            }

            if (typeFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Type", addonFilterDto?.Type));
            }
        }

        public async Task SoftDeleteAddonAsync(Guid id)
        {
            var query = @"
               DELETE FROM [GPA].[Inventory].[ProductAddons]
               WHERE AddonId = @Id;

               DELETE FROM [GPA].[Inventory].[Addons]
               WHERE Id = @Id;
                    ";

            await _context.Database
                .ExecuteSqlRawAsync(query, new SqlParameter("@Id", id));
        }

        public async Task<IEnumerable<RawProductByAddonId>> GetProductsByAddonIdAsync(Guid addonId, RequestFilterDto filter)
        {
            var (productByAddonIdFilterDto, termFilter, selectedFilter) = SetProductByAddonFilterParametersIfNotEmpty(filter);

            var query = $@"
                SELECT 
                     PRO.[Id]
                    ,PRO.[Code]
                    ,PRO.[Name]
                    ,PRO.[Photo]
                    ,PRO.[Price]
                    ,PRO.[Description]
	                ,CAST(IIF(PROADD.Id IS NULL, 0,1) AS BIT) AS IsSelected
                FROM [GPA].[Inventory].[Products] PRO
	                LEFT JOIN [GPA].[Inventory].[ProductAddons] PROADD ON 
		                    PRO.Id = PROADD.ProductId 		                 
		                AND PROADD.AddonId = @AddonId
                WHERE 
	                PRO.Deleted = 0 
                    AND PRO.[Type] = 1
	                {termFilter}
	                {selectedFilter}
                ORDER BY PRO.Id
	                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY  
            ";

            List<SqlParameter> parameters = new();
            var (Page, PageSize, _) = PagingHelper.GetPagingParameter(filter);
            AddProductByAddonFilterParameters(productByAddonIdFilterDto, termFilter, selectedFilter, parameters);

            parameters.AddRange([Page, PageSize, new SqlParameter("@AddonId", addonId)]);
            return await _context.Database.SqlQueryRaw<RawProductByAddonId>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetProductsCountByAddonIdAsync(Guid addonId, RequestFilterDto filter)
        {
            var (productByAddonIdFilterDto, termFilter, selectedFilter) = SetProductByAddonFilterParametersIfNotEmpty(filter);

            var query = @$"
                SELECT 
                    COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Products] PRO
	                LEFT JOIN [GPA].[Inventory].[ProductAddons] PROADD ON 
		                    PRO.Id = PROADD.ProductId 
		                AND PROADD.AddonId = @AddonId
                WHERE 
	                PRO.Deleted = 0 
		            AND PRO.[Type] = 1 
	                {termFilter}
	                {selectedFilter}
            ";
            List<SqlParameter> parameters = new() { new SqlParameter("@AddonId", addonId) };
            AddProductByAddonFilterParameters(productByAddonIdFilterDto, termFilter, selectedFilter, parameters);
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task AssignAddonToProductAsync(Guid addonId, Guid productId, Guid createdBy)
        {
            var query = @$"
                IF(NOT EXISTS(SELECT 1 FROM [GPA].[Inventory].[ProductAddons] WHERE ProductId = @ProductId AND AddonId = @AddonId ))
                BEGIN
                    {GetAssignAddonQuery(productId)}

	                INSERT INTO [GPA].[Inventory].[ProductAddons]([ProductId], [AddonId], [CreatedBy], [CreatedAt], Deleted)
	                VALUES(@ProductId, @AddonId, @CreatedBy, GETUTCDATE(), 0);
                END
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@ProductId", Value = productId },
                new SqlParameter() { ParameterName = "@AddonId", Value = addonId },
                new SqlParameter() { ParameterName = "@CreatedBy", Value = createdBy },
                new SqlParameter() { ParameterName = "@Action", Value = ActionConstants.Assign }
            });
        }

        public async Task RemoveAddonFromProductAsync(Guid addonId, Guid productId, Guid createdBy)
        {
            var query = @$"

                {GetAssignAddonQuery(productId)}

                DELETE FROM [GPA].[Inventory].[ProductAddons]
                WHERE ProductId = @ProductId AND AddonId = @AddonId
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@ProductId", Value = productId },
                new SqlParameter() { ParameterName = "@AddonId", Value = addonId },
                new SqlParameter() { ParameterName = "@CreatedBy", Value = createdBy },
                new SqlParameter() { ParameterName = "@Action", Value = ActionConstants.UnAssign }
            });
        }

        public async Task AssignAddonToAllProductAsync(Guid addonId, Guid createdBy)
        {
            var query = @$"
                INSERT INTO [GPA].[Inventory].[ProductAddons](ProductId, AddonId, CreatedAt, CreatedBy, Deleted)
                SELECT 
                     PRO.[Id]
                    ,@AddonId
                    ,GETUTCDATE()
                    ,@CreatedBy
                    ,0
                FROM [GPA].[Inventory].[Products] PRO
                    LEFT JOIN [GPA].[Inventory].[ProductAddons] PROADD ON 
                            PRO.Id = PROADD.ProductId 
                        AND PRO.[Type] = 1 
                        AND PROADD.AddonId = @AddonId	
                WHERE 
	                PROADD.Id IS NULL;

                {GetAssignAddonQuery()}
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@AddonId", Value = addonId },
                new SqlParameter() { ParameterName = "@CreatedBy", Value = createdBy },
                new SqlParameter() { ParameterName = "@Action", Value = ActionConstants.AssignAll }
            });
        }


        public async Task RemoveAddonFromAllProductAsync(Guid addonId, Guid createdBy)
        {
            var query = @$"
                {GetAssignAddonQuery()}
                
                DELETE FROM [GPA].[Inventory].[ProductAddons]
                WHERE AddonId = @AddonId
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@AddonId", Value = addonId },
                new SqlParameter() { ParameterName = "@CreatedBy", Value =  createdBy},
                new SqlParameter() { ParameterName = "@Action", Value =  ActionConstants.UnAssignAll},
            });
        }

        public async Task AddHistory(Addon addon, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[AddonHistory]
                   (
                    [Concept]
                   ,[IsDiscount]
                   ,[Type]
                   ,[Value]
                   ,[IdentityId]
                   ,[Action]
                   ,[By]
                   ,[At])
                VALUES
                   (@Concept
                   ,@IsDiscount
                   ,@Type
                   ,@Value
                   ,@IdentityId
                   ,@Action
                   ,@By
                   ,@At)
                    ";

            var parameters = new SqlParameter[]
            {
                new("@Concept", addon.Concept)
               ,new("@IsDiscount", addon.IsDiscount)
               ,new("@Type", addon.Type)
               ,new("@Value", addon.Value)
               ,new("@IdentityId", addon.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }

        private string GetAssignAddonQuery(Guid? productId = null)
        {
            var where = "";
            if (productId is not null)
            {
                where += " AND PRO.Id = @ProductId ";
            }

            return @$"
                INSERT INTO [Audit].[ProductAddonHistory]
                    ([Concept]
                    ,[ProductCode]
                    ,[ProductName]
                    ,[IdentityId]
                    ,[Action]
                    ,[By]
                    ,[At])
                SELECT 
	                 ISNULL([ADD].Concept,AD.Concept)
	                ,PRO.Code
	                ,PRO.[Name]
	                ,PRO.[Id]
	                ,@Action
	                ,@CreatedBy
	                ,GETUTCDATE()
                FROM [GPA].[Inventory].[Products] PRO
	                LEFT JOIN [GPA].[Inventory].[ProductAddons] PROADD ON 
			                PRO.Id = PROADD.ProductId 
		                AND PRO.[Type] = 1 
		                AND PROADD.AddonId = @AddonId
	                LEFT JOIN [GPA].[Inventory].[Addons] [ADD] ON 
		                [ADD].Id = PROADD.AddonId
                    JOIN [GPA].[Inventory].[Addons] AD ON AD.Id = @AddonId
                WHERE 1 = 1 {where} ;
            ";
        }

        private (ProductByAddonIdFilterDto? productByAddonIdFilterDto, string termFilter, string selectedFilter) SetProductByAddonFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var productByAddonIdFilterDto = new ProductByAddonIdFilterDto();
            if (filter.Search is { Length: > 0 })
            {
                productByAddonIdFilterDto = JsonSerializer.Deserialize<ProductByAddonIdFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = productByAddonIdFilterDto?.Term is { Length: > 0 } ? "AND (PRO.[Code] LIKE CONCAT('%', @Term, '%') OR PRO.[Name] LIKE CONCAT('%', @Term, '%'))" : "";
            var selectedFilter = productByAddonIdFilterDto?.Selected is not null && productByAddonIdFilterDto?.Selected != -1 ? "AND (@IsSelected IS NULL OR CAST(IIF(PROADD.Id IS NULL, 0,1) AS BIT) = @IsSelected)" : "";

            return (productByAddonIdFilterDto, termFilter, selectedFilter);
        }

        private void AddProductByAddonFilterParameters(ProductByAddonIdFilterDto? productByAddonIdFilterDto, string termFilter, string selectedFilter, List<SqlParameter> parameters)
        {
            if (termFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Term", productByAddonIdFilterDto?.Term));
            }

            if (selectedFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@IsSelected", productByAddonIdFilterDto?.Selected));
            }
        }
    }
}

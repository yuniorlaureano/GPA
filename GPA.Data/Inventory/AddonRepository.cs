using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Dtos.Inventory;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
              WHER Id = @Id
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

    }
}

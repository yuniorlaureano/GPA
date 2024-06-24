using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IAddonRepository : IRepository<Addon>
    {
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
    }
}

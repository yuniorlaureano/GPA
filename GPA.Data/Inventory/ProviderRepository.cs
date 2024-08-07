using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Task<RawProviders?> GetProviderByIdAsync(Guid id);
        Task<IEnumerable<RawProviders>> GetProvidersAsync(RequestFilterDto filter);
        Task<int> GetProvidersCountAsync(RequestFilterDto filter);
    }

    public class ProviderRepository : Repository<Provider>, IProviderRepository
    {
        public ProviderRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawProviders?> GetProviderByIdAsync(Guid id)
        {
            var query = @"
                SELECT
	               [Id]
                  ,[Name]
                  ,[LastName]
                  ,[Identification]
                  ,[IdentificationType]
                  ,[Phone]
                  ,[Email]
                  ,[Street]
                  ,[BuildingNumber]
                  ,[City]
                  ,[State]
                  ,[Country]
                  ,[PostalCode]
              FROM [GPA].[Inventory].[Providers] 
              WHERE Id = @Id
            ";

            return await _context.Database.SqlQueryRaw<RawProviders>(query, new SqlParameter("@Id", id)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProviders>> GetProvidersAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT
	               [Id]
                  ,[Name]
                  ,[LastName]
                  ,[Identification]
                  ,[IdentificationType]
                  ,[Phone]
                  ,[Email]
                  ,[Street]
                  ,[BuildingNumber]
                  ,[City]
                  ,[State]
                  ,[Country]
                  ,[PostalCode]
              FROM [GPA].[Inventory].[Providers] 
              WHERE
	              @Search IS NULL
	              OR CONCAT([Name], ' ', [LastName]) LIKE CONCAT('%', @Search, '%')
	              OR Identification LIKE CONCAT('%', @Search, '%')
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawProviders>(query, Page, PageSize, Search).ToListAsync();
        }


        public async Task<int> GetProvidersCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Providers] 
                WHERE
	              @Search IS NULL
	              OR CONCAT([Name], ' ', [LastName]) LIKE CONCAT('%', @Search, '%')
	              OR Identification LIKE CONCAT('%', @Search, '%')
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }
    }
}

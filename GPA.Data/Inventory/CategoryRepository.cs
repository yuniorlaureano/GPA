using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<RawCategory?> GetCategoryAsync(Guid id);
        Task<IEnumerable<RawCategory>> GetCategoriesAsync(RequestFilterDto filter);
        Task<int> GetCategoriesCountAsync(RequestFilterDto filter);
        Task SoftDeleteCategoryAsync(Guid categoryId);
    }

    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawCategory?> GetCategoryAsync(Guid id)
        {
            var query = @"
                SELECT
	                 [Id]
                    ,[Name]
                    ,[Description]
                    ,[Deleted]
                FROM [GPA].[Inventory].[Categories]
                WHERE Id = @Id AND Deleted = 0
            ";

            return await _context.Database.SqlQueryRaw<RawCategory>(
                query,
                new SqlParameter("@Id", id)
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawCategory>> GetCategoriesAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT
	                 [Id]
                    ,[Name]
                    ,[Description]
                    ,[Deleted]
                FROM [GPA].[Inventory].[Categories]
                WHERE Deleted = 0 AND (
                    @Search IS NULL
                    OR [Name] LIKE CONCAT('%', @Search, '%'))
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawCategory>(query, Page, PageSize, Search).ToListAsync();
        }


        public async Task<int> GetCategoriesCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT
	                COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Categories]
                WHERE Deleted = 0 AND (
                    @Search IS NULL
                    OR [Name] LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task SoftDeleteCategoryAsync(Guid categoryId)
        {
            var query = @"
                UPDATE [GPA].[Inventory].[Categories] 
                    SET [Deleted] = 1
                WHERE Id = @Id";

            await _context.Database.ExecuteSqlRawAsync(
                query,
                new SqlParameter("@Id", categoryId));
        }
    }
}

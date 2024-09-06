using GPA.Common.DTOs;
using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.General
{
    public interface IUnitRepository : IRepository<Unit>
    {
        Task SoftDeleteUnitAsync(Guid unitId);
        Task<int> GetUnitsCountAsync(RequestFilterDto filter);
        Task<IEnumerable<RawUnit>> GetUnitsAsync(RequestFilterDto filter);
        Task<RawUnit?> GetUnitAsync(Guid id);
    }

    public class UnitRepository : Repository<Unit>, IUnitRepository
    {
        public UnitRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawUnit?> GetUnitAsync(Guid id)
        {
            var query = @"
                SELECT
	                 [Id]
                    ,[Name]
                    ,[Code]
                    ,[Description]
                    ,[Deleted]
                FROM [GPA].[General].[Units]
                WHERE Id = @Id AND Deleted = 0
            ";

            return await _context.Database.SqlQueryRaw<RawUnit>(
                query,
                new SqlParameter("@Id", id)
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawUnit>> GetUnitsAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT
	                 [Id]
                    ,[Name]
                    ,[Code]
                    ,[Description]
                    ,[Deleted]
                FROM [GPA].[General].[Units]
                WHERE Deleted = 0 AND (
                    @Search IS NULL
                    OR [Name] LIKE CONCAT('%', @Search, '%')
                    OR [Code] LIKE CONCAT('%', @Search, '%')
                    OR [Description] LIKE CONCAT('%', @Search, '%'))
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawUnit>(query, Page, PageSize, Search).ToListAsync();
        }

        public async Task<int> GetUnitsCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT
	                COUNT(1) AS [Value]
                FROM [GPA].[General].[Units]
                WHERE Deleted = 0 AND (
                    @Search IS NULL
                    OR [Name] LIKE CONCAT('%', @Search, '%')
                    OR [Code] LIKE CONCAT('%', @Search, '%')
                    OR [Description] LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task SoftDeleteUnitAsync(Guid unitId)
        {
            var query = @"
                UPDATE [GPA].[General].[Units] 
                    SET [Deleted] = 1
                WHERE Id = @Id";

            await _context.Database.ExecuteSqlRawAsync(
                query,
                new SqlParameter("@Id", unitId));
        }
    }
}

using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Entities.Unmapped;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Security
{
    public interface IGPAUserRepository : IRepository<GPAUser>
    {
        Task<RawUser?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<RawUser>> GetUsersAsync(RequestFilterDto filter);
        Task<int> GetUsersCountAsync(RequestFilterDto filter);
        Task<bool> IsUserActive(Guid id);
    }

    public class GPAUserRepository : Repository<GPAUser>, IGPAUserRepository
    {
        public GPAUserRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<IEnumerable<RawUser>> GetUsersAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                Id,
	                FirstName,
	                LastName,
	                UserName,
	                Photo,
	                Email,
	                Invited,
	                Deleted,
                    EmailConfirmed,
                    CAST(0 AS BIT) AS IsAssigned
                FROM [GPA].[Security].[Users]
                WHERE (
	              @Search IS NULL
	              OR CONCAT(FirstName, ' ', LastName) LIKE CONCAT('%', @Search, '%')
	              OR UserName LIKE CONCAT('%', @Search, '%')
	              OR Email LIKE CONCAT('%', @Search, '%'))
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawUser>(query, Page, PageSize, Search).ToListAsync();
        }

        public async Task<RawUser?> GetUserByIdAsync(Guid id)
        {
            var query = @"
                SELECT 
	                Id,
	                FirstName,
	                LastName,
	                UserName,
	                Photo,
	                Email,
                    Invited,
	                Deleted,
                    EmailConfirmed,
                    CAST(0 AS BIT ) IsAssigned
                FROM [GPA].[Security].[Users]
                WHERE 
                    Id = @Id    
            ";

            return await _context.Database.SqlQueryRaw<RawUser>(query, new SqlParameter("@Id", id)).FirstOrDefaultAsync();
        }

        public async Task<int> GetUsersCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Security].[Users]
                WHERE (  
	              @Search IS NULL
	              OR CONCAT(FirstName, ' ', LastName) LIKE CONCAT('%', @Search, '%')
	              OR UserName LIKE CONCAT('%', @Search, '%')
	              OR Email LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task<bool> IsUserActive(Guid id)
        {
            return await _context.Users.AnyAsync(x => x.Id == id && !x.Deleted);
        }
    }
}

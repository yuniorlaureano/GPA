using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Entities.Unmapped;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Security
{
    public interface IGPAProfileRepository : IRepository<GPAProfile>
    {
        Task<int> GetProfilesCountAsync(RequestFilterDto filter);
        Task<RawProfile?> GetProfilesByIdAsync(Guid id);
        Task<IEnumerable<RawProfile>> GetProfilesAsync(RequestFilterDto filter);
        Task AssignProfileToUser(Guid profileId, Guid userId, Guid createdBy);
        Task<List<RawUserProfile>> GetProfilesByUserId(List<Guid> userIds);
        Task<List<RawUser>> GetUsers(Guid profileId, int page, int pageSize);
        Task<int> GetUsersCount();
        Task UnAssignProfileFromUser(Guid profileId, Guid userId);
        Task<List<RawProfile>> GetProfilesByUserId(Guid userId);
        Task<bool> ProfileExists(Guid profileId, Guid userId);
        Task<bool> ProfileExists(Guid userId);
        Task<string?> GetProfileValue(Guid profileId);
    }

    public class GPAProfileRepository : Repository<GPAProfile>, IGPAProfileRepository
    {
        public GPAProfileRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task AssignProfileToUser(Guid profileId, Guid userId, Guid createdBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ProfileId", profileId),
                new SqlParameter("@CreatedBy", createdBy)
            };

            await _context.Database.ExecuteSqlRawAsync(
              @"IF NOT EXISTS (
	                SELECT 1 FROM [GPA].[Security].[GPAUserProfiles]
	                WHERE
		                [UserId] = @UserId AND
		                [UserId] = @ProfileId
                ) BEGIN
	                INSERT INTO [GPA].[Security].[GPAUserProfiles]
	                (
		                 [UserId]
		                ,[ProfileId]
		                ,[CreatedBy]
                        ,[Deleted]
                        ,[CreatedAt]
	                ) VALUES(
		                 @UserId,
		                 @ProfileId,
		                 @CreatedBy,
                         0,
                         GETUTCDATE()   
	                )
                END", parameters);
        }

        public async Task<List<RawUserProfile>> GetProfilesByUserId(List<Guid> userIds)
        {
            return await _context.Database.SqlQueryRaw<RawUserProfile>(
              @$"SELECT [Id]
                      ,[UserId]
                      ,[ProfileId]
                  FROM [GPA].[Security].[GPAUserProfiles]
                  WHERE 
	                [UserId] IN (SELECT value FROM STRING_SPLIT(@userIds, ','))
                ", new SqlParameter("@userIds", string.Join(",", userIds.ToArray()))).ToListAsync();
        }

        public async Task<List<RawUser>> GetUsers(Guid profileId, int page, int pageSize)
        {
            var query = @$"
              SELECT 
                 USR.[Id]
                ,USR.[FirstName]
                ,USR.[LastName]
                ,USR.[Deleted]
                ,USR.[UserName]
                ,USR.[Email]
	            ,CAST(IIF(USRP.ProfileId IS NULL, 0, 1) AS BIT) AS IsAssigned
              FROM [GPA].[Security].[GPAUsers] USR
	            LEFT JOIN [GPA].[Security].[GPAUserProfiles] USRP
		            ON USR.Id = USRP.UserId AND
		               USRP.ProfileId = @ProfileId
              ORDER BY 7 desc,USRP.Id
              OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY";

            return await _context.Database.SqlQueryRaw<RawUser>(
                  query,
                  new SqlParameter("@ProfileId", profileId),
                  new SqlParameter("@Page", pageSize * Math.Abs(page - 1)),
                  new SqlParameter("@PageSize", pageSize)
              ).ToListAsync();
        }

        public async Task<int> GetUsersCount()
        {
            return await _context.Users.CountAsync();
        }

        public async Task UnAssignProfileFromUser(Guid profileId, Guid userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ProfileId", profileId)
            };

            await _context.Database.ExecuteSqlRawAsync(
              @"DELETE FROM [GPA].[Security].[GPAUserProfiles]
                WHERE
	                [UserId] = @UserId AND
	                [ProfileId] = @ProfileId", parameters);
        }

        public async Task<List<RawProfile>> GetProfilesByUserId(Guid userId)
        {
            var query = @$"
              SELECT DISTINCT
	                 P.[Id],
	                 P.[Name],
	                 P.[Value]
                FROM [GPA].[Security].[GPAProfiles] P
	                JOIN [GPA].[Security].[GPAUserProfiles] USRP
		                ON P.Id = USRP.ProfileId AND
		                   USRP.UserId = @UserId";

            return await _context.Database.SqlQueryRaw<RawProfile>(
                  query,
                  new SqlParameter("@UserId", userId)
              ).ToListAsync();
        }

        public async Task<bool> ProfileExists(Guid profileId, Guid userId)
        {
            return await _context.UserProfile.AnyAsync(x => x.ProfileId == profileId && x.UserId == userId);
        }

        public async Task<bool> ProfileExists(Guid userId)
        {
            return await _context.UserProfile.AnyAsync(x => x.UserId == userId);
        }

        public async Task<string?> GetProfileValue(Guid profileId)
        {
            return await _context.Profiles
                .Where(x => x.Id == profileId)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProfile>> GetProfilesAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 [Id]
                    ,[Name]
                    ,[Value]
                FROM [GPA].[Security].[GPAProfiles]
                WHERE 
	              @Search IS NULL
	              OR [Name] LIKE CONCAT('%', @Search, '%')
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawProfile>(query, Page, PageSize, Search).ToListAsync();
        }

        public async Task<RawProfile?> GetProfilesByIdAsync(Guid id)
        {
            var query = @"
                SELECT 
	                 [Id]
                    ,[Name]
                    ,[Value]
                FROM [GPA].[Security].[GPAProfiles]
                WHERE 
                    Id = @Id    
            ";

            return await _context.Database.SqlQueryRaw<RawProfile>(query, new SqlParameter("@Id", id)).FirstOrDefaultAsync();
        }

        public async Task<int> GetProfilesCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Security].[GPAProfiles]
                WHERE 
	              @Search IS NULL
	              OR [Name] LIKE CONCAT('%', @Search, '%')
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }
    }
}

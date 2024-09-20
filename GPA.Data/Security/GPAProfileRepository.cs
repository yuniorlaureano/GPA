using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Entities.Unmapped;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GPA.Data.Security
{
    public interface IGPAProfileRepository : IRepository<GPAProfile>
    {
        Task<int> GetProfilesCountAsync(RequestFilterDto filter);
        Task<RawProfile?> GetProfilesByIdAsync(Guid id);
        Task<IEnumerable<RawProfile>> GetProfilesAsync(RequestFilterDto filter);
        Task AssignProfileToUser(Guid profileId, Guid userId, Guid createdBy);
        Task<List<RawUserProfile>> GetProfilesByUserId(List<Guid> userIds);
        Task<List<RawUser>> GetUsers(Guid profileId, RequestFilterDto filter);
        Task<int> GetUsersCount(RequestFilterDto filter);
        Task UnAssignProfileFromUser(Guid profileId, Guid userId);
        Task<List<RawProfile>> GetProfilesByUserId(Guid userId);
        Task<bool> ProfileExists(Guid profileId, Guid userId);
        Task<bool> ProfileExists(Guid userId);
        Task<(string? value, bool? isDeleted)?> GetProfileValue(Guid profileId, Guid userId);
        Task AddHistory(GPAProfile profile, string action, Guid by);
        Task AddUserProfileHistory(Guid userId, Guid profileId, string action, Guid by);
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
	                SELECT 1 FROM [GPA].[Security].[UserProfiles]
	                WHERE
		                [UserId] = @UserId AND
		                [ProfileId] = @ProfileId
                ) BEGIN
	                INSERT INTO [GPA].[Security].[UserProfiles]
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
                  FROM [GPA].[Security].[UserProfiles]
                  WHERE 
	                [UserId] IN (SELECT value FROM STRING_SPLIT(@userIds, ','))
                ", new SqlParameter("@userIds", string.Join(",", userIds.ToArray()))).ToListAsync();
        }

        public async Task<List<RawUser>> GetUsers(Guid profileId, RequestFilterDto filter)
        {
            var query = @$"
              SELECT 
                 USR.[Id]
                ,USR.[FirstName]
                ,USR.[LastName]
                ,USR.[Deleted]
                ,USR.[Photo]
                ,USR.[UserName]
                ,USR.[Email]
                ,USR.Invited
                ,USR.EmailConfirmed
	            ,CAST(IIF(USRP.ProfileId IS NULL, 0, 1) AS BIT) AS IsAssigned
              FROM [GPA].[Security].[Users] USR
	            LEFT JOIN [GPA].[Security].[UserProfiles] USRP
		            ON USR.Id = USRP.UserId AND
		               USRP.ProfileId = @ProfileId
              WHERE 
                  USR.Deleted = 0 AND (
	                  @Search IS NULL
	                  OR CONCAT(USR.FirstName, ' ', USR.LastName) LIKE CONCAT('%', @Search, '%')
	                  OR USR.UserName LIKE CONCAT('%', @Search, '%')
	                  OR USR.Email LIKE CONCAT('%', @Search, '%'))  
              ORDER BY 7 desc,USRP.Id
              OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawUser>(
                  query,
                  new SqlParameter("@ProfileId", profileId),
                  query, Page, PageSize, Search
              ).ToListAsync();
        }

        public async Task<int> GetUsersCount(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Security].[Users]
                WHERE 
                  Deleted = 0 AND (  
	              @Search IS NULL
	              OR CONCAT(FirstName, ' ', LastName) LIKE CONCAT('%', @Search, '%')
	              OR UserName LIKE CONCAT('%', @Search, '%')
	              OR Email LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task UnAssignProfileFromUser(Guid profileId, Guid userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ProfileId", profileId)
            };

            await _context.Database.ExecuteSqlRawAsync(
              @"DELETE FROM [GPA].[Security].[UserProfiles]
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
                FROM [GPA].[Security].[Profiles] P
	                JOIN [GPA].[Security].[UserProfiles] USRP
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

        public async Task<(string? value, bool? isDeleted)?> GetProfileValue(Guid profileId, Guid userId)
        {
            var query = @"
                SELECT TOP 1
                    @Profile = P.[Value],
                    @UserDeleted = U.[Deleted]
                FROM [Security].[Profiles] P
	                JOIN  [Security].[UserProfiles] UP
		                ON P.Id = UP.ProfileId AND P.Id = @ProfileId
	                JOIN [Security].[Users] U ON UP.UserId = U.Id AND U.Id = @UserId
            ";

            var userDeletedParameter = new SqlParameter("@UserDeleted", SqlDbType.Bit);
            userDeletedParameter.Direction = ParameterDirection.Output;

            var profileParameter = new SqlParameter("@Profile", SqlDbType.NVarChar, -1);
            profileParameter.Direction = ParameterDirection.Output;

            var profileIdParameter = new SqlParameter("@ProfileId", SqlDbType.UniqueIdentifier);
            profileIdParameter.Direction = ParameterDirection.Input;
            profileIdParameter.Value = profileId;

            var userIdParameter = new SqlParameter("@UserId", SqlDbType.UniqueIdentifier);
            userIdParameter.Direction = ParameterDirection.Input;
            userIdParameter.Value = userId;

            var rows = await _context.Database.ExecuteSqlRawAsync(
                query,
                profileIdParameter,
                userIdParameter,
                userDeletedParameter, 
                profileParameter);
            
            var isDeleted = userDeletedParameter.Value as bool?;
            var profileValue = profileParameter.Value as string;

            if (string.IsNullOrEmpty(profileValue))
            {
                return null;
            }

            return (profileValue, isDeleted);
        }

        public async Task<IEnumerable<RawProfile>> GetProfilesAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 [Id]
                    ,[Name]
                    ,[Value]
                FROM [GPA].[Security].[Profiles]
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
                FROM [GPA].[Security].[Profiles]
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
                FROM [GPA].[Security].[Profiles]
                WHERE 
	              @Search IS NULL
	              OR [Name] LIKE CONCAT('%', @Search, '%')
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task AddHistory(GPAProfile profile, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[ProfileHistory]
                       ([Name]
                       ,[Value]
                       ,[IdentityId]
                       ,[Action]
                       ,[By]
                       ,[At])
                 VALUES
                       (@Name
                       ,@Value
                       ,@IdentityId
                       ,@Action
                       ,@By
                       ,@At)
                    ";

            var parameters = new SqlParameter[]
            {
                new("@Name", profile.Name)
               ,new("@Value", profile.Value ?? "")
               ,new("@IdentityId", profile.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }

        public async Task AddUserProfileHistory(Guid userId, Guid profileId, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[UserProfileHistory]
                    ([UserId]
                    ,[ProfileId]
                    ,[IdentityId]
                    ,[Action]
                    ,[By]
                    ,[At])
                SELECT 
                     [UserId]
                    ,[ProfileId]
	                ,[Id]
	                ,@Action
	                ,@By
	                ,@At
                FROM [GPA].[Security].[UserProfiles]
                WHERE UserId = @UserId AND ProfileId = @ProfileId
                    ";

            var parameters = new SqlParameter[]
            {
                new("@UserId", userId)
               ,new("@ProfileId", profileId)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }
    }
}

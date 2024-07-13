using GPA.Common.Entities.Security;
using GPA.Entities.Unmapped;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Security
{
    public interface IGPAProfileRepository : IRepository<GPAProfile>
    {
        Task AssignProfileToUser(Guid profileId, Guid userId, Guid createdBy);
        Task<List<RawUserProfile>> GetProfilesByUserId(List<Guid> userIds);
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
	                ) VALUES(
		                 @UserId,
		                 @ProfileId,
		                 @CreatedBy,
                         0
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
    }
}

using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Dtos.Security;
using GPA.Entities.Security;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Security;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data.Security
{
    public interface IGPAUserRepository : IRepository<GPAUser>
    {
        Task<RawUser?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<RawUser>> GetUsersAsync(RequestFilterDto filter);
        Task<int> GetUsersCountAsync(RequestFilterDto filter);
        Task<bool> IsUserActive(Guid id);
        Task<RawInvitationToken?> GetInvitationTokenAsync(Guid id);
        Task AddInvitationTokenAsync(InvitationToken invitationToken);
        Task SetInvitationTokenAsync(Guid id, string token);
        Task RedeemInvitationAsync(Guid userId);
        Task RevokeInvitationAsync(Guid id, Guid revokedBy);
        Task<IEnumerable<RawInvitationToken>> GetInvitationTokensAsync(Guid userId);
        Task<IEnumerable<RawUser>> GetUsersAsync(List<Guid> ids);
        Task<Dictionary<Guid, RawUser>> GetUsersAsDictionaryAsync(List<Guid> ids);
    }

    public class GPAUserRepository : Repository<GPAUser>, IGPAUserRepository
    {
        public GPAUserRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<IEnumerable<RawUser>> GetUsersAsync(RequestFilterDto filter)
        {
            var (userFilterDto, termFilter, confirmFilter, invitedFilter) = SetUserFilterParametersIfNotEmpty(filter);

            var query = $@"
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
                WHERE 1 = 1
                    {termFilter}
                    {confirmFilter}
                    {invitedFilter}
                ORDER BY Id DESC
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var parameters = new List<SqlParameter>();

            var (Page, PageSize, _) = PagingHelper.GetPagingParameter(filter);
            parameters.AddRange([Page, PageSize]);
            AddUserFilterParameters(userFilterDto, termFilter, confirmFilter, invitedFilter, parameters);
            return await _context.Database.SqlQueryRaw<RawUser>(query, parameters.ToArray()).ToListAsync();
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
            var (userFilterDto, termFilter, confirmFilter, invitedFilter) = SetUserFilterParametersIfNotEmpty(filter);
            var query = $@"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Security].[Users]
                WHERE 1 = 1
                    {termFilter}
                    {confirmFilter}
                    {invitedFilter}
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            AddUserFilterParameters(userFilterDto, termFilter, confirmFilter, invitedFilter, parameters);
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task<bool> IsUserActive(Guid id)
        {
            return await _context.Users.AnyAsync(x => x.Id == id && !x.Deleted);
        }

        public async Task<RawInvitationToken?> GetInvitationTokenAsync(Guid id)
        {
            var query = @"
                SELECT 
	               [Id]
                  ,[Token]
                  ,[Expiration]
                  ,[UserId]
                  ,[Revoked]
                  ,[CreatedBy]
                  ,[CreatedAt]
                  ,[Redeemed]
                  ,RevokedBy
	              ,RevokedAt
                FROM [GPA].[Security].[InvitationTokens]
                WHERE 
                        [Id] = @Id 
            ";

            return await _context.Database.SqlQueryRaw<RawInvitationToken>(
                query,
                new SqlParameter("@Id", id)
            ).FirstOrDefaultAsync();
        }

        public async Task AddInvitationTokenAsync(InvitationToken invitationToken)
        {
            _context.InvitationTokens.Add(invitationToken);
            await _context.SaveChangesAsync();
            _context.Entry(invitationToken).State = EntityState.Detached;
        }

        public async Task SetInvitationTokenAsync(Guid id, string token)
        {
            await _context.InvitationTokens
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(
                    x => x.SetProperty(p => p.Token, token));

            await _context.SaveChangesAsync();
        }

        public async Task RedeemInvitationAsync(Guid userId)
        {
            await _context.InvitationTokens
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Redeemed, true));

            await _context.SaveChangesAsync();
        }

        public async Task RevokeInvitationAsync(Guid id, Guid revokedBy)
        {
            await _context.InvitationTokens
                .Where(x => x.Id == id && !x.Revoked)
                .ExecuteUpdateAsync(
                    x => x.SetProperty(p => p.Revoked, true)
                          .SetProperty(p => p.RevokedBy, revokedBy)
                          .SetProperty(p => p.RevokedAt, DateTimeOffset.UtcNow));

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RawInvitationToken>> GetInvitationTokensAsync(Guid userId)
        {
            var query = @"
                SELECT 
	               [Id]
                  ,[Token]
                  ,[Expiration]
                  ,[UserId]
                  ,[Revoked]
                  ,[CreatedBy]
                  ,[CreatedAt]
                  ,[Redeemed]
                  ,RevokedBy
	              ,RevokedAt
                FROM [GPA].[Security].[InvitationTokens]
                WHERE 
                        [UserId] = @Id 
            ";

            return await _context.Database.SqlQueryRaw<RawInvitationToken>(
                query,
                new SqlParameter("@Id", userId)
            ).ToListAsync();
        }

        public async Task<IEnumerable<RawUser>> GetUsersAsync(List<Guid> ids)
        {
            if (!ids.Any())
            {
                return Enumerable.Empty<RawUser>();
            }

            var userIds = ids.Select(x => $"'{x}'");
            var query = $@"
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
                WHERE Id IN({string.Join(",", userIds)})  
            ";

            return await _context.Database.SqlQueryRaw<RawUser>(query).ToListAsync();
        }

        public async Task<Dictionary<Guid, RawUser>> GetUsersAsDictionaryAsync(List<Guid> ids)
        {
            if (!ids.Any())
            {
                return new Dictionary<Guid, RawUser>();
            }

            var userIds = ids.Select(x => $"'{x}'");
            var query = $@"
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
                WHERE Id IN({string.Join(",", userIds)})  
            ";

            var results = await _context.Database.SqlQueryRaw<RawUser>(query).ToListAsync();
            var users = new Dictionary<Guid, RawUser>();
            foreach (var item in results)
            {
                if (!users.ContainsKey(item.Id))
                {
                    users.Add(item.Id, item);
                }
            }
            return users;
        }

        private (UserFilterDto? userFilterDto, string termFilter, string confirmFilter, string invitedFilter) SetUserFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var userFilterDto = new UserFilterDto()
            {
                Term = "",
                Confirm = -1,
                Invited = -1
            };

            if (filter.Search is { Length: > 0 })
            {
                userFilterDto = JsonSerializer.Deserialize<UserFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = userFilterDto?.Term is { Length: > 0 } ? @"AND (
                  CONCAT(FirstName, ' ', LastName) LIKE CONCAT('%', @Term, '%')
	              OR UserName LIKE CONCAT('%', @Term, '%')
	              OR Email LIKE CONCAT('%', @Term, '%')
                )" : "";
            var invitedFilter = userFilterDto?.Invited != -1 ? "AND Invited = @Invited" : "";
            var confirmFilter = userFilterDto?.Confirm != -1 ? "AND EmailConfirmed = @Confirm" : "";

            return (userFilterDto, termFilter, confirmFilter, invitedFilter);
        }

        private void AddUserFilterParameters(UserFilterDto? userFilterDto, string termFilter, string confirmFilter, string invitedFilter, List<SqlParameter> parameters)
        {
            if (termFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Term", userFilterDto?.Term));
            }

            if (confirmFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Confirm", userFilterDto?.Confirm == 0 ? false : true));
            }

            if (invitedFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Invited", userFilterDto?.Invited == 0 ? false : true));
            }
        }
    }
}

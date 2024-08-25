using GPA.Common.DTOs;
using GPA.Common.Entities.Invoice;
using GPA.Entities.Unmapped.Invoice;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GPA.Data.Invoice
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<RawClient> GetClientAsync(Guid clientId);
        Task<int> GetClientsCountAsync(RequestFilterDto filter);
        Task<IEnumerable<RawClient>> GetClientsAsync(RequestFilterDto filter);
        Task<IEnumerable<RawClient>> GetClientsByIdsAsync(IEnumerable<Guid> clientIds);
        Task UpdateAsync(Client client);
        Task<IEnumerable<RawCredit>> GetCreditsByClientIdAsync(List<Guid> clientIds);
        Task SoftDeleteClientAsync(Guid clientId, Guid createdBy);
    }

    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawClient?> GetClientAsync(Guid clientId)
        {
            var query = @$"
                SELECT   [Id]
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
                        ,[FormattedAddress] 
                        ,[Latitude]
                        ,[Longitude]
                FROM [GPA].[Invoice].[Clients] C
                WHERE C.Id = @ClientId";

            return await _context.Database.SqlQueryRaw<RawClient>(query, new SqlParameter("@ClientId", clientId)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawClient>> GetClientsByIdsAsync(IEnumerable<Guid> clientIds)
        {
            var query = @$"
                  SELECT [Id]
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
                        ,[FormattedAddress]
                        ,[Latitude]
                        ,[Longitude]
                FROM [GPA].[Invoice].[Clients]
                WHERE C.Id IN ({string.Join(",", clientIds.Select(clientId => $"'{clientId}'"))})";

            return await _context.Database.SqlQueryRaw<RawClient>(query).ToListAsync();
        }

        public async Task<IEnumerable<RawClient>> GetClientsAsync(RequestFilterDto filter)
        {
            var term = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var search = term is { Length: > 0 } ? "AND (CONCAT(C.[Name],' ',C.LastName) LIKE CONCAT('%', @Search, '%') OR C.Identification LIKE CONCAT('%', @Search, '%'))" : "";

            var query = @$"SELECT [Id]
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
                    ,[FormattedAddress]
                    ,[Latitude]
                    ,[Longitude]
                FROM [GPA].[Invoice].[Clients] C
                WHERE
                    Deleted = 0
                    {search}
                ORDER BY C.Id
                OFFSET @Page ROWS
                FETCH NEXT @PageSize ROWS ONLY ";

            var (Page, PageSize, _) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            if (!string.IsNullOrEmpty(search))
            {
                parameters.Add(new SqlParameter("@Search", term));
            }

            return await _context.Database.SqlQueryRaw<RawClient>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetClientsCountAsync(RequestFilterDto filter)
        {
            var term = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var search = filter.Search is { Length: > 0 } ? "AND (CONCAT(C.[Name],' ',C.LastName) LIKE CONCAT('%', @Search, '%') OR C.Identification LIKE CONCAT('%', @Search, '%'))" : "";

            var query = @$"SELECT 
                               COUNT(1) AS [Value]
                        FROM [GPA].[Invoice].[Clients] C
                        WHERE Deleted = 0
                            {search}
                        ";

            var parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(search))
            {
                parameters.Add(new SqlParameter("@Search", term));
            }

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).CountAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            await _context.ClientCredits.Where(x => x.ClientId == client.Id).ExecuteDeleteAsync();
            _context.Client.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RawCredit>> GetCreditsByClientIdAsync(List<Guid> clientIds)
        {
            var query = @$"SELECT
	                     [Id]
                        ,[Credit]
                        ,[Concept]
                        ,[ClientId]
                    FROM [GPA].[Invoice].[ClientCredits]
                    WHERE [ClientId] IN({string.Join(",", clientIds.Select(clientId => $"'{clientId}'"))})";

            return await _context.Database.SqlQueryRaw<RawCredit>(query).ToListAsync();
        }

        public async Task SoftDeleteClientAsync(Guid clientId, Guid createdBy)
        {
            var query = @$"
                UPDATE [GPA].[Invoice].[Clients] 
                    SET Deleted = 1,
                        DeletedAt = @DeletedAt,
                        DeletedBy = @DeletedBy
                WHERE 
                    Id = @Id";

            await _context.Database.ExecuteSqlRawAsync(
                query,
                new SqlParameter("@DeletedBy", createdBy),
                new SqlParameter("@DeletedAt", DateTimeOffset.UtcNow),
                new SqlParameter("@Id", clientId)
                );
        }
    }
}

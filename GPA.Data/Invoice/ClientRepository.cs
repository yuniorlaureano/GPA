using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped.Audit;
using GPA.Entities.Unmapped.Invoice;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

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
        Task AddHistory(Client client, List<ClientCredit?> clientCredit, string action, Guid by);
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
                FROM [GPA].[Invoice].[Clients]
                WHERE Id = @ClientId";

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
                WHERE Id IN ({string.Join(",", clientIds.Select(clientId => $"'{clientId}'"))})";

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

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
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

        public async Task AddHistory(Client client, List<ClientCredit?> clientCredit, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[ClientHistory]
                   ([Name]
                   ,[Identification]
                   ,[Phone]
                   ,[Email]
                   ,[Address]
                   ,[FormattedAddress]
                   ,[Latitude]
                   ,[Longitude]
                   ,[CreditsHistory]
                   ,[IdentityId]
                   ,[Action]
                   ,[By]
                   ,[At])
             VALUES
                   (@Name
                   ,@Identification
                   ,@Phone
                   ,@Email
                   ,@Address
                   ,@FormattedAddress
                   ,@Latitude
                   ,@Longitude
                   ,@CreditsHistory
                   ,@IdentityId
                   ,@Action
                   ,@By
                   ,@At)
                    ";

            var clientCredits = clientCredit?.Select(x => new ClientCreditHistory
            {
                Credit = x.Credit,
                Concept = x.Concept
            });

            var parameters = new List<SqlParameter>
            {
                new("@Name", client.Name)
               ,new("@Identification", $"{GetIdentificationTypePrefix(client.IdentificationType)} {client.Identification}")
               ,new("@Phone", client.Phone ?? "")
               ,new("@Email", client.Email ?? "")
               ,new("@Address", $"#{client.BuildingNumber}, C/{client.Street}, {client.State}, {client.City}, {client.Country}, {client.PostalCode}")
               ,new("@FormattedAddress", client.FormattedAddress)
               ,new("@Latitude", client.Latitude ?? 0D)
               ,new("@Longitude", client.Longitude ?? 0D)
               ,new("@CreditsHistory", clientCredits?.Any() == true ? JsonSerializer.Serialize(clientCredits) : "")
               ,new("@IdentityId", client.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }

        private string GetIdentificationTypePrefix(IdentificationType identificationType)
        {
            return identificationType switch
            {
                IdentificationType.Cedula => "Cédula",
                IdentificationType.RNC => "RNC",
                IdentificationType.Passport => "Pasaporte",
                _ => "",
            };
        }
    }
}

using GPA.Common.DTOs;
using GPA.Common.Entities.Invoice;
using GPA.Entities.Unmapped;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GPA.Data.Invoice
{
    public interface IReceivableAccountRepository : IRepository<ClientPaymentsDetails>
    {
        Task<IEnumerable<ClientPaymentsDetailSummary>> GetReceivableSummaryAsync(RequestFilterDto filter);
        Task<int> GetReceivableSummaryCountAsync(RequestFilterDto filter);
        Task<IEnumerable<RawPenddingPayment>> GetPenddingPaymentByClientId(Guid clientId);
    }

    public class ReceivableAccountRepository : Repository<ClientPaymentsDetails>, IReceivableAccountRepository
    {
        public ReceivableAccountRepository(GPADbContext _dbContext) : base(_dbContext)
        {

        }

        public async Task<IEnumerable<ClientPaymentsDetailSummary>> GetReceivableSummaryAsync(RequestFilterDto filter)
        {
            var search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            search = filter.Search is { Length: > 0 } ? "AND CONCAT(C.[Name],' ',C.LastName) LIKE CONCAT('%', @Search, '%')" : "";

            var query = @$"SELECT 
                     CD.[InvoiceId]
	                ,CONCAT(C.[Name],' ',C.LastName) AS Client
                    ,max(CD.[PendingPayment]) AS PendingPayment 
                    ,sum(CD.[Payment]) AS Payment
                FROM [GPA].[Invoice].[ClientPaymentsDetails] CD
	                JOIN Invoice.Invoices INV ON CD.InvoiceId =  INV.Id AND INV.[Status] = 1
	                JOIN Invoice.Clients C ON INV.ClientId =  C.Id
                WHERE 1 = 1
                    {search}
                group by 
	                [InvoiceId]
	                ,CONCAT(C.[Name],' ',C.LastName)
                ORDER BY CD.[InvoiceId]
                OFFSET @Page ROWS
                FETCH NEXT @PageSize ROWS ONLY ";

            var (Page, PageSize, _) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            if (!string.IsNullOrEmpty(search))
            {
                parameters.Add(new SqlParameter("@Search", search));
            }

            return await _context.Database.SqlQueryRaw<ClientPaymentsDetailSummary>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<IEnumerable<RawPenddingPayment>> GetPenddingPaymentByClientId(Guid clientId)
        {
            return await _context.Database.SqlQueryRaw<RawPenddingPayment>(
                @"SELECT [Id]
                      ,[PendingPayment]
                      ,[Payment]
	                  ,InvoiceId
                  FROM [GPA].[Invoice].[ClientPaymentsDetails]
                  WHERE InvoiceId IN (
	                SELECT Id FROM [GPA].[Invoice].[Invoices]
	                WHERE ClientId = {0} AND PaymentStatus  = 1 AND [Status] = 1
                  ) AND Payment = 0", clientId).ToListAsync();

        }

        public async Task<int> GetReceivableSummaryCountAsync(RequestFilterDto filter)
        {
            var search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            search = filter.Search is { Length: > 0 } ? "AND CONCAT(C.[Name],' ',C.LastName) LIKE CONCAT('%', @Search, '%')" : "";

            var query = @$"SELECT 
                               COUNT(1) AS [Value]
                        FROM [GPA].[Invoice].[ClientPaymentsDetails] CD
	                        JOIN Invoice.Invoices INV ON CD.InvoiceId =  INV.Id AND INV.[Status] = 1
	                        JOIN Invoice.Clients C ON INV.ClientId =  C.Id
                        WHERE 1 = 1
                            {search}
                        group by 
	                        [InvoiceId]
	                        ,CONCAT(C.[Name],' ',C.LastName) ";

            var parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(search))
            {
                parameters.Add(new SqlParameter("@Search", search));
            }

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }
    }
}

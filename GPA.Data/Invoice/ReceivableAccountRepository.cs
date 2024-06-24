using GPA.Common.Entities.Invoice;
using GPA.Entities.Unmapped;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Invoice
{
    public interface IReceivableAccountRepository : IRepository<ClientPaymentsDetails>
    {
        Task<IEnumerable<ClientPaymentsDetailSummary>> GetReceivableSummaryAsync(int page = 1, int pageSize = 10);
        Task<int> GetReceivableSummaryCountAsync();
        Task<IEnumerable<RawPenddingPayment>> GetPenddingPaymentByClientId(Guid clientId);
    }

    public class ReceivableAccountRepository : Repository<ClientPaymentsDetails>, IReceivableAccountRepository
    {
        public ReceivableAccountRepository(GPADbContext _dbContext) : base(_dbContext)
        {

        }

        public async Task<IEnumerable<ClientPaymentsDetailSummary>> GetReceivableSummaryAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Database
                .SqlQuery<ClientPaymentsDetailSummary>(
                @$"SELECT 
                     CD.[InvoiceId]
	                ,CONCAT(C.[Name],' ',C.LastName) AS Client
                    ,max(CD.[PendingPayment]) AS PendingPayment 
                    ,sum(CD.[Payment]) AS Payment
                FROM [GPA].[Invoice].[ClientPaymentsDetails] CD
	                JOIN Invoice.Invoices INV ON CD.InvoiceId =  INV.Id
	                JOIN Invoice.Clients C ON INV.ClientId =  C.Id
                group by 
	                [InvoiceId]
	                ,CONCAT(C.[Name],' ',C.LastName)
                ORDER BY CD.[InvoiceId]
                OFFSET {pageSize * Math.Abs(page - 1)} ROWS
                FETCH NEXT {pageSize} ROWS ONLY ").ToListAsync();

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
	                WHERE ClientId = {0} AND PaymentStatus  = 1
                  ) AND Payment = 0", clientId).ToListAsync();

        }

        public async Task<int> GetReceivableSummaryCountAsync()
        {
            return await _context.Database
                .SqlQuery<int>(
                @$"SELECT 
		                    1 AS [Count]
	                    FROM [GPA].[Invoice].[ClientPaymentsDetails] CD
	                    GROUP BY [InvoiceId] ").CountAsync();

        }
    }
}

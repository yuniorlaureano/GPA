using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GPA.Data.General
{

    public interface IDashboardRepository
    {
        Task<int> GetClientsCount();
        Task<decimal> GetSelesRevenue(int month = 0);
        Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence();
        Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason);

    }

    public class DashboardRepository : IDashboardRepository
    {
        private readonly GPADbContext _context;
        public DashboardRepository(GPADbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<int> GetClientsCount()
        {
            var query = @$"
                SELECT COUNT(1) AS [Value]
                FROM [GPA].[Invoice].[Clients]
                WHERE Deleted = 0 
            ";

            return await _context.Database.SqlQueryRaw<int>(query).FirstOrDefaultAsync();
        }

        public async Task<decimal> GetSelesRevenue(int month = 0)
        {
            var currentMonth = month <= 0 ? DateTime.Now.Month: month;
            var query = @"
                SELECT ISNULL(SUM(X.Revenue),0) AS [Value] FROM (
	                SELECT 
		                INV.Id,
		                INV.Payment + SUM(isnull(CPD.Payment,0)) AS Revenue
	                FROM [GPA].[Invoice].[Invoices] INV
		                LEFT JOIN [GPA].[Invoice].[ClientPaymentsDetails] CPD ON INV.Id = CPD.InvoiceId
	                WHERE 
		                 INV.[Status] = 1 
		                 AND MONTH(INV.CreatedAt) = @Month
	                GROUP BY INV.Id,INV.Payment
                ) X
            ";
            return await _context.Database.SqlQueryRaw<decimal>(query, new SqlParameter("@Month", currentMonth)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence()
        {
            var query = @"
                SELECT
	                SUM(CASE
			                WHEN [t0].[TransactionType] IS NULL THEN 0
			                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                ELSE [t].[Quantity]
	                END) AS [Existence],
	                SUM(CASE
			                WHEN [t0].[TransactionType] IS NULL THEN 0
			                WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                ELSE 0
	                END) AS [Input],
	                SUM(CASE
			                WHEN [t0].[TransactionType] IS NULL THEN 0
			                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                ELSE 0
	                END) AS [Output],
	                MAX([p].[Price]) AS [Price],
	                MAX([P].[Type]) AS [ProductType],
	                [p].[Id] AS [ProductId]
                FROM 
	                [Inventory].[Products] AS [p]
	                LEFT JOIN [Inventory].[StockDetails] [t] ON [p].[Id] = [t].[ProductId] AND [t].[Deleted] = CAST(0 AS bit)
	                LEFT JOIN [Inventory].[Stocks] AS [t0] 
		                ON [t].[StockId] = [t0].[Id] AND 
		                    [t0].[Deleted] = CAST(0 AS bit) AND 
		                    ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) AND 
		                    ([t0].[Status] <> 2 OR [t0].[Status] IS NULL)
                WHERE 
	                [p].[Deleted] = CAST(0 AS bit) 
                GROUP BY 
	                [p].[Id]";

            return await _context.Database.SqlQueryRaw<RawInputVsOutputVsExistence>(query).ToListAsync();
        }

        public async Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason)
        {
            var query = @"
            DECLARE 
	            @Months AS TABLE(Mth INT);
            DECLARE
	            @Year INT = YEAR(GETUTCDATE());

            INSERT INTO @Months(Mth) VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12);

            SELECT 
	            MT.Mth AS [Month],
	            SUM(CASE WHEN ST.Id IS NULL THEN 0 ELSE STD.Quantity END) AS Quantity
            FROM @Months MT
	            LEFT JOIN [Inventory].[Stocks] ST
		            ON     @Year  = YEAR(ST.[Date])
		               AND MT.Mth = MONTH(ST.[Date])
	            LEFT JOIN [Inventory].[StockDetails] STD
		            ON ST.Id = STD.StockId
            WHERE     (ST.ReasonId =	   @ReasonId OR ST.Id IS NULL)
                    AND (ST.[Status] =	   1 OR ST.Id IS NULL)
            GROUP BY 
	            MT.Mth
            ";

            var reasonParam = new SqlParameter("@ReasonId", SqlDbType.SmallInt);
            reasonParam.Value = (byte)reason;

            return await _context.Database.SqlQueryRaw<RawTransactionsPerMonthByReason>(
                    query,
                    reasonParam
                ).ToListAsync();
        }
    }
}

using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.General
{

    public interface IDashboardRepository
    {
        Task<int> GetClientsCount();
        Task<int> GetSelesRevenue();
        Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence();
        Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason, TransactionType transactionType);

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
                SELECT COUNT(1)
                FROM [GPA].[Invoice].[Clients]
                WHERE Deleted = 0 
            ";

            var parameters = new List<SqlParameter>();
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public Task<int> GetSelesRevenue()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence()
        {
            var query = @"
                SELECT
	                SUM(CASE
			                WHEN [t0].[TransactionType] IS NULL THEN 0
			                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                ELSE [t].[Quantity]
	                END) AS [Stock],
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

        public async Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason, TransactionType transactionType)
        {
            var query = @"
            DECLARE 
	            @Months AS TABLE(Mth SMALLINT);
            DECLARE
	            @Year SMALLINT = YEAR(GETUTCDATE());

            INSERT INTO @Months(Mth) VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12);

            SELECT 
	            MT.Mth AS [Month],
	            SUM(CASE WHEN ST.Id IS NULL THEN 0 ELSE STD.Quantity END) AS Quantity
            FROM @Months MT
	            LEFT JOIN [Inventory].[Stocks] ST
		            ON     @Year  = YEAR(ST.CreatedAt)
		               AND MT.Mth = MONTH(ST.CreatedAt)
	            LEFT JOIN [Inventory].[StockDetails] STD
		            ON ST.Id = STD.StockId
            WHERE ST.TransactionType = @TransactionType 
	              AND ST.ReasonId =	   @ReasonId 
            GROUP BY 
	            MT.Mth
            ";

            return await _context.Database.SqlQueryRaw<RawTransactionsPerMonthByReason>(
                    query,
                    new SqlParameter("@TransactionType", transactionType),
                    new SqlParameter("@ReasonId", reason)
                ).ToListAsync();
        }
    }
}

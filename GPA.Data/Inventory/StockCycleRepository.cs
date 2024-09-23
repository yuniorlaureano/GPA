using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Dtos.Inventory;
using GPA.Entities.General;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data.Inventory
{
    public interface IStockCycleRepository : IRepository<StockCycle>
    {
        Task<RawStockCycle?> GetStockCycleAsync(Guid id);
        Task<IEnumerable<RawStockCycleDetails>> GetStockCycleDetailsAsync(Guid id);
        Task<IEnumerable<RawStockCycle>> GetStockCyclesAsync(RequestFilterDto filter);
        Task<int> GetStockCycleCountAsync(RequestFilterDto filter);
        Task<Guid> OpenCycleAsync(StockCycle model);
        Task CloseCycleAsync(Guid id, Guid updatedBy);
        Task SoftDeleteStockCycleAsync(Guid id, Guid createdBy);
    }

    public class StockCycleRepository : Repository<StockCycle>, IStockCycleRepository
    {
        public StockCycleRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<Guid> OpenCycleAsync(StockCycle model)
        {
            await _context.Database.OpenConnectionAsync();
            using var command = _context.Database.GetDbConnection().CreateCommand();

            var insert = @"
                DECLARE @OutputTable TABLE (Id uniqueidentifier); 

                INSERT INTO [GPA].[Inventory].[StockCycles]([Note], [StartDate], [EndDate], [CreatedBy], [CreatedAt], [IsClose])
	                OUTPUT INSERTED.Id INTO @OutputTable
                VALUES(@Note, @StartDate, @EndDate, @CreatedBy, GETUTCDATE(), 0)

                SELECT @InsertedId=Id FROM @OutputTable

                INSERT INTO [GPA].[Inventory].[StockCycleDetails](
	                 [ProductId]
                    ,[ProductPrice]
                    ,[ProductName]
                    ,[ProductType]
                    ,[Stock]
                    ,[Input]
                    ,[Output]
                    ,[StockCycleId]
                    ,[Type])
                SELECT 
	                [p].[Id] AS [ProductId],
	                MAX([p].[Price]) AS [Price],
	                [p].[Name] AS [ProductName],
	                MAX([P].[Type]) AS [ProductType],
	                SUM(CASE
			                WHEN [t].[Id] IS NULL THEN 0
			                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                ELSE [t].[Quantity]
	                END) AS [Stock],
	                SUM(CASE
			                WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                ELSE 0
	                END) AS [Input],
	                SUM(CASE
			                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                ELSE 0
	                END) AS [Output],
	                @InsertedId,
                    @CycleType
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
                    AND CONVERT(date, [t0].[Date]) >= @StartDate
                GROUP BY 
	                [p].[Id], 
	                [p].[Name]
                ";

            var insertedId = new SqlParameter("@InsertedId", dbType: System.Data.SqlDbType.UniqueIdentifier)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            command.CommandText = insert;
            command.Parameters.AddRange(
                   new[]
                   {
                        new ("@CreatedBy", System.Data.SqlDbType.UniqueIdentifier) { Value = model.CreatedBy },
                        new ("@StartDate", System.Data.SqlDbType.Date) { Value = model.StartDate },
                        new ("@EndDate", System.Data.SqlDbType.Date) { Value = model.EndDate },
                        new ("@Note", System.Data.SqlDbType.NVarChar) { Value = model.Note },
                        new ("@CycleType", System.Data.SqlDbType.TinyInt) { Value = (byte)CycleType.Initial },
                        insertedId
                   }
                );

            await command.ExecuteNonQueryAsync();
            var insertedCycleId = (Guid)insertedId.Value;
            await _context.Database.CloseConnectionAsync();
            return insertedCycleId;
        }

        public async Task CloseCycleAsync(Guid id, Guid updatedBy)
        {
            await _context.Database.OpenConnectionAsync();
            using var command = _context.Database.GetDbConnection().CreateCommand();

            var insert = @"
                IF EXISTS (SELECT 1 FROM [GPA].[Inventory].[StockCycles] WHERE Id = @CycleId AND IsClose = 0)
                BEGIN
                    DECLARE @EndDate DATE;
                    SELECT TOP 1 @EndDate=EndDate FROM [GPA].[Inventory].[StockCycles] WHERE Id = @CycleId

	                UPDATE [GPA].[Inventory].[StockCycles]
	                SET [IsClose] = 1,
						[UpdatedBy] = @UpdatedBy,
						[UpdatedAt] = @UpdatedAt			
	                WHERE Id = @CycleId

	                INSERT INTO [GPA].[Inventory].[StockCycleDetails](
		                 [ProductId]
		                ,[ProductPrice]
		                ,[ProductName]
		                ,[ProductType]
		                ,[Stock]
		                ,[Input]
		                ,[Output]
		                ,[StockCycleId]
		                ,[Type])
	                SELECT 
		                [p].[Id] AS [ProductId],
		                MAX([p].[Price]) AS [Price],
		                [p].[Name] AS [ProductName],
		                MAX([P].[Type]) AS [ProductType],
		                SUM(CASE
				                WHEN [t].[Id] IS NULL THEN 0
				                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
				                ELSE [t].[Quantity]
		                END) AS [Stock],
		                SUM(CASE
				                WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
				                ELSE 0
		                END) AS [Input],
		                SUM(CASE
				                WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
				                ELSE 0
		                END) AS [Output],
		                @CycleId,
		                @CycleType
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
                        AND CONVERT(date, [t0].[Date]) <= @EndDate
	                GROUP BY 
		                [p].[Id], 
		                [p].[Name]
                END
                ";

            command.CommandText = insert;
            command.Parameters.AddRange(new SqlParameter[]
                   {
                        new ("@CycleId", System.Data.SqlDbType.UniqueIdentifier) { Value = id },
                        new ("@CycleType", System.Data.SqlDbType.TinyInt) { Value = (byte)CycleType.Final },
                        new ("@UpdatedBy", System.Data.SqlDbType.UniqueIdentifier) { Value = updatedBy },
                        new ("@UpdatedAt", System.Data.SqlDbType.DateTimeOffset) { Value = DateTimeOffset.UtcNow },
                   }
                );

            await command.ExecuteNonQueryAsync();
            await _context.Database.CloseConnectionAsync();
        }

        public async Task<RawStockCycle?> GetStockCycleAsync(Guid id)
        {
            var query = @"
                SELECT 
	                 [Id]
                    ,[Note]
                    ,[StartDate]
                    ,[EndDate]
                    ,[IsClose]
                FROM [GPA].[Inventory].[StockCycles]
                WHERE Id = @Id AND Deleted = 0
                    ";

            return await _context.Database
                .SqlQueryRaw<RawStockCycle>(query, new SqlParameter("@Id", id))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawStockCycleDetails>> GetStockCycleDetailsAsync(Guid id)
        {
            var query = @"                
              SELECT 
                 STD.[Id]
                ,STD.[ProductId]
                ,STD.[ProductPrice]
                ,STD.[ProductName]
                ,P.[Code] AS ProductCode
                ,STD.[ProductType]
                ,STD.[Stock]
                ,STD.[Input]
                ,STD.[Output]
                ,STD.[Type]
                ,STD.[StockCycleId]
            FROM [GPA].[Inventory].[StockCycleDetails] STD
	            JOIN [Inventory].[Products] P ON STD.ProductId = P.Id
                    ";

            return await _context.Database
                .SqlQueryRaw<RawStockCycleDetails>(query, new SqlParameter("@StockCycleId", id))
                .ToListAsync();
        }

        public async Task<IEnumerable<RawStockCycle>> GetStockCyclesAsync(RequestFilterDto filter)
        {
            var (DateFilter, IsCloseFilter, StockCycleFilter) = GetFilter(filter);

            var query = @$"
                SELECT 
	                 [Id]
                    ,[Note]
                    ,[StartDate]
                    ,[EndDate]
                    ,[IsClose]
                FROM [GPA].[Inventory].[StockCycles]
                WHERE Deleted = 0 
                    {DateFilter}
                    {IsCloseFilter}
                ORDER BY Id DESC
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
                    ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var (From, To, IsClose) = GetFilterParameter(StockCycleFilter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            if (DateFilter is { Length: > 0 })
            {
                parameters.AddRange([From, To]);
            }
            if (IsCloseFilter is { Length: > 0 })
            {
                parameters.Add(IsClose);
            }

            return await _context.Database.SqlQueryRaw<RawStockCycle>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetStockCycleCountAsync(RequestFilterDto filter)
        {
            var (DateFilter, IsCloseFilter, StockCycleFilter) = GetFilter(filter);

            var query = @$"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[StockCycles]
                WHERE Deleted = 0
                    {DateFilter}
                    {IsCloseFilter}
                    ";

            var (From, To, IsClose) = GetFilterParameter(StockCycleFilter);
            var parameters = new List<SqlParameter>();

            if (DateFilter is { Length: > 0 })
            {
                parameters.AddRange([From, To]);
            }
            if (IsCloseFilter is { Length: > 0 })
            {
                parameters.Add(IsClose);
            }
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        private (SqlParameter From, SqlParameter To, SqlParameter IsClose) GetFilterParameter(StockCycleListFilter? stockCycleFilter)
        {
            var from = new SqlParameter("@From", DetailedDateUtil.GetDetailedDate(stockCycleFilter?.From));
            var to = new SqlParameter("@To", DetailedDateUtil.GetDetailedDate(stockCycleFilter?.To));
            var isClose = new SqlParameter("@IsClose", stockCycleFilter?.IsClose == -1 ? null : stockCycleFilter?.IsClose);
            from.DbType = System.Data.DbType.Date;
            to.DbType = System.Data.DbType.Date;
            isClose.DbType = System.Data.DbType.Boolean;

            return (from, to, isClose);
        }

        private (string DateFilter, string IsCloseFilter, StockCycleListFilter? StockCycleFilter) GetFilter(RequestFilterDto filter)
        {
            var search = SearchHelper.ConvertSearchToString(filter);
            var stockCycleListFilter = new StockCycleListFilter()
            {
                DateTypeFilter = "",
                From = null,
                To = null,
            };

            if (search is { Length: > 0 })
            {
                stockCycleListFilter = JsonSerializer.Deserialize<StockCycleListFilter>(search, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var dateField = "EndDate";
            if (stockCycleListFilter?.DateTypeFilter == "init")
            {
                dateField = "StartDate";
            }

            var dateFilter = stockCycleListFilter?.From is null || stockCycleListFilter?.To is null ? "" : $"AND {dateField} BETWEEN @From AND @To";
            var isCloseFilter = stockCycleListFilter?.IsClose is null || stockCycleListFilter?.IsClose == -1 ? "" : "AND IsClose = @IsClose";
            return (dateFilter, isCloseFilter, stockCycleListFilter);
        }

        public async Task SoftDeleteStockCycleAsync(Guid id, Guid createdBy)
        {
            var query = @"
                UPDATE [GPA].[Inventory].[StockCycles]
                SET 
	                Deleted = 1,
	                DeletedAt = GETUTCDATE(),
	                DeletedBy = @DeletedBy
                WHERE Id = @Id
                    ";

            await _context.Database
                .ExecuteSqlRawAsync(
                    query,
                    new SqlParameter("@Id", id),
                    new SqlParameter("@DeletedBy", createdBy));
        }
    }
}

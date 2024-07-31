using GPA.Common.Entities.Inventory;
using GPA.Entities.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IStockCycleRepository : IRepository<StockCycle>
    {
        Task<Guid> OpenCycleAsync(StockCycle model);
		Task CloseCycleAsync(Guid id, Guid updatedBy);
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
                        new ("@CreatedBy", System.Data.SqlDbType.NVarChar) { Value = model.CreatedBy },
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
    }
}

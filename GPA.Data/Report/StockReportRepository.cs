using GPA.Common.DTOs;
using GPA.Dtos.Inventory;
using GPA.Entities.Unmapped;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data.Inventory
{
    public interface IStockReportRepository
    {
        Task<IEnumerable<Existence>> GetAllExistenceAsync(RequestFilterDto filter);
    }

    public class StockReportRepository : IStockReportRepository
    {
        private readonly GPADbContext _context;

        public StockReportRepository(GPADbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Existence>> GetAllExistenceAsync(RequestFilterDto filter)
        {
            var (existenceFilterDto, termFilter, typeFilter) = SetExistenceFilterParametersIfNotEmpty(filter);

            var sqlQuery = @$"SELECT 
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
	                            [p].[CategoryId],
	                            [p].[Code] AS [ProductCode],
	                            concat([p].[Name], ' ' , p.UnitValue, ' ' ,UNT.[Name] ) AS [ProductName],
	                            [p].[Id] AS [ProductId]
                            FROM 
	                            [Inventory].[Products] AS [p]
                                JOIN [GPA].[General].[Units] UNT ON p.UnitId = UNT.Id
	                            LEFT JOIN [Inventory].[StockDetails] [t] ON [p].[Id] = [t].[ProductId] AND [t].[Deleted] = CAST(0 AS bit)
	                            LEFT JOIN [Inventory].[Stocks] AS [t0] 
		                            ON [t].[StockId] = [t0].[Id] AND 
		                                [t0].[Deleted] = CAST(0 AS bit) AND 
		                                ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) AND 
		                                ([t0].[Status] <> 2 OR [t0].[Status] IS NULL)
                            WHERE 
	                            [p].[Deleted] = CAST(0 AS bit) 
                                {termFilter}
                                {typeFilter}
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId],
                                p.UnitValue,
								UNT.[Name]";

            var parameters = new List<SqlParameter>();
            AddExistenceFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<Existence>(sqlQuery, parameters.ToArray()).ToListAsync();
        }


        private (ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter) SetExistenceFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var existenceFilterDto = new ExistenceFilterDto();
            if (filter.Search is { Length: > 0 })
            {
                existenceFilterDto = JsonSerializer.Deserialize<ExistenceFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = existenceFilterDto?.Term is { Length: > 0 } ? "AND ([p].[Code] LIKE CONCAT('%', @Term, '%') OR [p].[Name] LIKE CONCAT('%', @Term, '%'))" : "";
            var typeFilter = existenceFilterDto?.Type != -1 && existenceFilterDto?.Type is not null ? "AND [P].[Type] = @Type" : "";

            return (existenceFilterDto, termFilter, typeFilter);
        }

        private void AddExistenceFilterParameters(ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter, List<SqlParameter> parameters)
        {
            if (termFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Term", existenceFilterDto?.Term));
            }

            if (typeFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Type", existenceFilterDto?.Type));
            }
        }
    }
}

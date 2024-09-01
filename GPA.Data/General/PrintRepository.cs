using GPA.Common.DTOs;
using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.General
{

    public interface IPrintRepository : IRepository<PrintInformation>
    {
        Task<RawPrintInformation?> GetPrintInformationByIdAsync(Guid id);
        Task<IEnumerable<RawPrintInformation>> GetPrintInformationAsync(RequestFilterDto filter);
        Task<RawPrintInformation?> GetCurrentPrintInformationAsync();
        Task<int> GetPrintInformationCountAsync(RequestFilterDto filter);
        Task SavePhoto(string fullFileName, Guid printInformationId);
    }

    public class PrintRepository : Repository<PrintInformation>, IPrintRepository
    {
        public PrintRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<RawPrintInformation?> GetPrintInformationByIdAsync(Guid id)
        {
            var query = @"
                SELECT 
	               [Id]
                  ,[CompanyLogo]
                  ,[CompanyName]
                  ,[CompanyDocument]
                  ,[CompanyAddress]
                  ,[CompanyPhone]
                  ,[CompanyEmail]
                  ,[CompanyWebsite]
                  ,[Signer]
                  ,[Current]
                  ,[StoreId]  
              FROM [GPA].[General].[PrintInformation]
              WHERE Id = @Id
            ";

            return await _context.Database.SqlQueryRaw<RawPrintInformation>(
                query,
                new SqlParameter("@Id", id)
            ).FirstOrDefaultAsync();
        }

        public async Task<RawPrintInformation?> GetCurrentPrintInformationAsync()
        {
            var query = @"
                SELECT TOP 1
	               [Id]
                  ,[CompanyLogo]
                  ,[CompanyName]
                  ,[CompanyDocument]
                  ,[CompanyAddress]
                  ,[CompanyPhone]
                  ,[CompanyEmail]
                  ,[CompanyWebsite]
                  ,[Signer]
                  ,[Current]
                  ,[StoreId]  
              FROM [GPA].[General].[PrintInformation]
              WHERE [Current] = 1
            ";

            return await _context.Database.SqlQueryRaw<RawPrintInformation>(query).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawPrintInformation>> GetPrintInformationAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	               [Id]
                  ,[CompanyLogo]
                  ,[CompanyName]
                  ,[CompanyDocument]
                  ,[CompanyAddress]
                  ,[CompanyPhone]
                  ,[CompanyEmail]
                  ,[CompanyWebsite]
                  ,[Signer]
                  ,[Current]
                  ,[StoreId]  
                FROM [GPA].[General].[PrintInformation]
                WHERE 1 = 1 AND (
	                @Search IS NULL
	                OR [CompanyName] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyDocument] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyAddress] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyPhone] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyEmail] LIKE CONCAT('%', @Search, '%')
	                OR [Signer] LIKE CONCAT('%', @Search, '%')
                )
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<RawPrintInformation>(query, Page, PageSize, Search).ToListAsync();
        }

        public async Task<int> GetPrintInformationCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[General].[PrintInformation]
                WHERE 1 = 1 AND (
	                @Search IS NULL
	                OR [CompanyName] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyDocument] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyAddress] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyPhone] LIKE CONCAT('%', @Search, '%')
	                OR [CompanyEmail] LIKE CONCAT('%', @Search, '%')
	                OR [Signer] LIKE CONCAT('%', @Search, '%')
                )
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            return await _context.Database.SqlQueryRaw<int>(query, Search).FirstOrDefaultAsync();
        }

        public async Task SavePhoto(string fullFileName, Guid printInformationId)
        {
            var query = @$"
                UPDATE [GPA].[General].[PrintInformation]
                SET [CompanyLogo] = @Photo 
                WHERE Id = @Id 
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter("@Photo", fullFileName), new SqlParameter("@Id", printInformationId));
        }
    }
}

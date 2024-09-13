using GPA.Entities.Report;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IReportTemplateRepository
    {
        Task<ReportTemplate?> GetTemplateByCode(string code);
    }

    public class ReportTemplateRepository : IReportTemplateRepository
    {
        private readonly GPADbContext _context;

        public ReportTemplateRepository(GPADbContext context)
        {
            _context = context;
        }

        public async Task<ReportTemplate?> GetTemplateByCode(string code)
        {
            var query = @$"SELECT 
	                            Id,
                                Code,
                                Template
                            FROM 
	                            [General].[ReportTemplate]
                            WHERE [Code] = @Code";

            var parameters = new List<SqlParameter>();

            return await _context.Database.SqlQueryRaw<ReportTemplate?>(
                query,
                new SqlParameter("@Code", code)
             ).FirstOrDefaultAsync();
        }
    }
}

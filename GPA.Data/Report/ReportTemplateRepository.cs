using GPA.Entities.Report;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IReportTemplateRepository
    {
        Task<ReportTemplate?> GetTemplateByCode(string code);
        Task<ReportTemplate?> GetTemplateById(Guid id);
        Task<IEnumerable<ReportTemplate>> GetTemplates();
        Task UpdateTemplate(Guid id, ReportTemplate reportTemplate);
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
                                Template,
                                Width,
                                Height,
                                UpdatedBy,
                                UpdatedAt
                            FROM 
	                            [General].[ReportTemplates]
                            WHERE [Code] = @Code";

            var parameters = new List<SqlParameter>();

            return await _context.Database.SqlQueryRaw<ReportTemplate?>(
                query,
                new SqlParameter("@Code", code)
             ).FirstOrDefaultAsync();
        }

        public async Task<ReportTemplate?> GetTemplateById(Guid id)
        {
            var query = @$"SELECT 
	                            Id,
                                Code,
                                Template,
                                Width,
                                Height,
                                UpdatedBy,
                                UpdatedAt
                            FROM 
	                            [General].[ReportTemplates]
                            WHERE [Id] = @Id";

            var parameters = new List<SqlParameter>();

            return await _context.Database.SqlQueryRaw<ReportTemplate?>(
                query,
                new SqlParameter("@Id", id)
             ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReportTemplate>> GetTemplates()
        {
            var query = @$"SELECT 
	                            Id,
                                Code,
                                Template,
                                Width,
                                Height,
                                UpdatedBy,
                                UpdatedAt
                            FROM 
	                            [General].[ReportTemplates]";

            var parameters = new List<SqlParameter>();

            return await _context.Database.SqlQueryRaw<ReportTemplate>(
                query
             ).ToListAsync();
        }


        public async Task UpdateTemplate(Guid id, ReportTemplate reportTemplate)
        {
            await _context.ReportTemplates.Where(x => x.Id == id)
                .ExecuteUpdateAsync(x => 
                    x.SetProperty(p => p.Template, reportTemplate.Template)
                     .SetProperty(p => p.Width, reportTemplate.Width)
                     .SetProperty(p => p.Height, reportTemplate.Height)
                     .SetProperty(p => p.UpdatedAt, reportTemplate.UpdatedAt)
                     .SetProperty(p => p.UpdatedBy, reportTemplate.UpdatedBy)
                 );
        }
    }
}

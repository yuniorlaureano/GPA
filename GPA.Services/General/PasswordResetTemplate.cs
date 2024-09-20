using GPA.Data.Inventory;
using GPA.Entities.Report;
using GPA.Utils;
using GPA.Utils.Caching;

namespace GPA.Services.General
{
    public interface IPasswordResetTemplate
    {
        Task<string> GetPasswordResetTemplate();
    }

    public class PasswordResetTemplate : IPasswordResetTemplate
    {
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IGenericCache<ReportTemplate> _cache;

        public PasswordResetTemplate(
            IReportTemplateRepository reportTemplateRepository,
            IGenericCache<ReportTemplate> cache
            )
        {
            _reportTemplateRepository = reportTemplateRepository;
            _cache = cache;
        }

        public async Task<string> GetPasswordResetTemplate()
        {
            var template = await GetTemplate();
            return template.Template;
        }

        private async Task<ReportTemplate> GetTemplate()
        {
            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.PASSWORD_RESET_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.PASSWORD_RESET_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template no existe ");
            }
            return template;
        }
    }
}

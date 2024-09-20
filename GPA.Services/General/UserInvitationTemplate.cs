using GPA.Data.Inventory;
using GPA.Entities.Report;
using GPA.Utils;
using GPA.Utils.Caching;

namespace GPA.Services.General
{
    public interface IUserInvitationTemplate
    {
        Task<string> GetUserInvitationEmailTemplate();
    }

    public class UserInvitationTemplate : IUserInvitationTemplate
    {
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IGenericCache<ReportTemplate> _cache;

        public UserInvitationTemplate(
            IReportTemplateRepository reportTemplateRepository,
            IGenericCache<ReportTemplate> cache
            )
        {
            _reportTemplateRepository = reportTemplateRepository;
            _cache = cache;
        }

        public async Task<string> GetUserInvitationEmailTemplate()
        {
            var template = await GetTemplate();
            return template.Template;
        }

        private async Task<ReportTemplate> GetTemplate()
        {
            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.USER_INVITATION_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.USER_INVITATION_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template no existe ");
            }
            return template;
        }
    }
}

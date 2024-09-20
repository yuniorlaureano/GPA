using GPA.Data.Inventory;
using GPA.Entities.Report;
using GPA.Utils.Caching;
using GPA.Utils;

namespace GPA.Services.General
{
    public interface IInvitationRedemptionTemplate
    {
        Task<string> GetInvitationRedemptionTemplate();
    }

    public class InvitationRedemptionTemplate : IInvitationRedemptionTemplate
    {
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IGenericCache<ReportTemplate> _cache;

        public InvitationRedemptionTemplate(
            IReportTemplateRepository reportTemplateRepository,
            IGenericCache<ReportTemplate> cache
            )
        {
            _reportTemplateRepository = reportTemplateRepository;
            _cache = cache;
        }

        public async Task<string> GetInvitationRedemptionTemplate()
        {
            var template = await GetTemplate();
            return template.Template;
        }

        private async Task<ReportTemplate> GetTemplate()
        {
            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.USER_INVITATION_REDEMPTION_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.USER_INVITATION_REDEMPTION_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template no existe ");
            }
            return template;
        }
    }
}

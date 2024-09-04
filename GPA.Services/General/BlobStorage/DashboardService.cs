using GPA.Data.General;
using GPA.Entities.General;
using GPA.Entities.Unmapped.General;

namespace GPA.Services.General.BlobStorage
{
    public interface IDashboardService
    {
        Task<int> GetClientsCount();
        Task<decimal> GetSelesRevenue(int month = 0);
        Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence();
        Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<int> GetClientsCount()
        {
            return await _dashboardRepository.GetClientsCount();
        }

        public async Task<decimal> GetSelesRevenue(int month = 0)
        {
            return await _dashboardRepository.GetSelesRevenue(month);
        }

        public async Task<IEnumerable<RawInputVsOutputVsExistence>> GetInputVsOutputVsExistence()
        {
            return await _dashboardRepository.GetInputVsOutputVsExistence();
        }

        public async Task<IEnumerable<RawTransactionsPerMonthByReason>> GetTransactionsPerMonthByReason(ReasonTypes reason)
        {
            return await _dashboardRepository.GetTransactionsPerMonthByReason(reason);
        }
    }
}

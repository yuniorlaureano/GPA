using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IReasonRepository : IRepository<Reason>
    {
    }

    public class ReasonRepository : Repository<Reason>, IReasonRepository
    {
        public ReasonRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

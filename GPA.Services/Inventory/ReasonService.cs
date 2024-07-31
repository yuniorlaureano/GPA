using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IReasonService
    {
        public Task<ReasonDto?> GetByIdAsync(int id);

        public Task<ResponseDto<ReasonDto>> GetAllAsync(SearchDto search, Expression<Func<Reason, bool>>? expression = null);

        public Task<ReasonDto?> AddAsync(ReasonDto deasonDto);

        public Task UpdateAsync(ReasonDto deasonDto);

        public Task RemoveAsync(int id);
    }

    public class ReasonService : IReasonService
    {
        private readonly IReasonRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public ReasonService(
            IReasonRepository repository, 
            IUserContextService userContextService,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<ReasonDto?> GetByIdAsync(int id)
        {
            var deason = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ReasonDto>(deason);
        }

        public async Task<ResponseDto<ReasonDto>> GetAllAsync(SearchDto search, Expression<Func<Reason, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<ReasonDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ReasonDto>>(categories)
            };
        }

        public async Task<ReasonDto> AddAsync(ReasonDto dto)
        {
            var reason = _mapper.Map<Reason>(dto);
            reason.CreatedBy = _userContextService.GetCurrentUserId();
            reason.CreatedAt = DateTimeOffset.UtcNow;
            var savedReason = await _repository.AddAsync(reason);
            return _mapper.Map<ReasonDto>(savedReason);
        }

        public async Task UpdateAsync(ReasonDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newReason = _mapper.Map<Reason>(dto);
            newReason.Id = dto.Id.Value;
            newReason.UpdatedBy = _userContextService.GetCurrentUserId();
            newReason.UpdatedAt = DateTimeOffset.UtcNow;
            var savedReason = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedReason, newReason, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(int id)
        {
            var savedReason = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedReason);
        }
    }
}

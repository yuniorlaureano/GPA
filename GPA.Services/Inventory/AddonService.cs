﻿using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Services.Security;

namespace GPA.Business.Services.Inventory
{
    public interface IAddonService
    {
        Task<AddonDto?> GetAddonsAsync(Guid id);
        Task<ResponseDto<AddonDto>> GetAddonsAsync(RequestFilterDto search);
        Task<AddonDto?> AddAsync(AddonDto addonDto);
        Task UpdateAsync(AddonDto addonDto);
        Task RemoveAsync(Guid id);
        Task<ResponseDto<RawProductByAddonId>> GetProductsByAddonIdAsync(Guid addonId, RequestFilterDto filter);
        Task RemoveAddonFromProductAsync(Guid addonId, Guid productId);
        Task AssignAddonToProductAsync(Guid addonId, Guid productId);
        Task RemoveAddonFromAllProductAsync(Guid addonId);
        Task AssignAddonToAllProductAsync(Guid addonId);
    }

    public class AddonService : IAddonService
    {
        private readonly IAddonRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public AddonService(
            IAddonRepository repository,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<AddonDto?> GetAddonsAsync(Guid id)
        {
            var addon = await _repository.GetAddonsAsync(id);
            return _mapper.Map<AddonDto>(addon);
        }

        public async Task<ResponseDto<AddonDto>> GetAddonsAsync(RequestFilterDto search)
        {
            var addons = await _repository.GetAddonsAsync(search);
            return new ResponseDto<AddonDto>
            {
                Count = await _repository.GetAddonsCountAsync(search),
                Data = _mapper.Map<IEnumerable<AddonDto>>(addons)
            };
        }

        public async Task<AddonDto> AddAsync(AddonDto dto)
        {
            var addon = _mapper.Map<Addon>(dto);
            addon.CreatedBy = _userContextService.GetCurrentUserId();
            addon.CreatedAt = DateTimeOffset.UtcNow;
            var savedAddon = await _repository.AddAsync(addon);
            return _mapper.Map<AddonDto>(savedAddon);
        }

        public async Task UpdateAsync(AddonDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newAddon = _mapper.Map<Addon>(dto);
            newAddon.Id = dto.Id.Value;
            var savedAddon = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            newAddon.UpdatedBy = _userContextService.GetCurrentUserId();
            newAddon.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(savedAddon, newAddon, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }
        
        public async Task<ResponseDto<RawProductByAddonId>> GetProductsByAddonIdAsync(Guid addonId, RequestFilterDto filter)
        {
            return new ResponseDto<RawProductByAddonId>
            {
               Data = await _repository.GetProductsByAddonIdAsync(addonId, filter),
               Count = await _repository.GetProductsCountByAddonIdAsync(addonId, filter)
            };
        }

        public Task RemoveAddonFromProductAsync(Guid addonId, Guid productId)
        {
            return _repository.RemoveAddonFromProductAsync(addonId, productId);
        }

        public Task AssignAddonToProductAsync(Guid addonId, Guid productId)
        {
            return _repository.AssignAddonToProductAsync(addonId, productId, _userContextService.GetCurrentUserId());
        }

        public Task RemoveAddonFromAllProductAsync(Guid addonId)
        {
            return _repository.RemoveAddonFromAllProductAsync(addonId);
        }

        public Task AssignAddonToAllProductAsync(Guid addonId)
        {
            return _repository.AssignAddonToAllProductAsync(addonId, _userContextService.GetCurrentUserId());
        }

        public async Task RemoveAsync(Guid id)
        {
            await _repository.SoftDeleteAddonAsync(id);
        }
    }
}

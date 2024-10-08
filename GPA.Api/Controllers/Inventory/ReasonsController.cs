﻿using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Utils.Caching;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Inventory.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("inventory/[controller]")]
    [ApiController()]
    public class ReasonsController : ControllerBase
    {
        private readonly IReasonService _reasonService;
        private readonly IGenericCache<ResponseDto<ReasonDto>> _cache;
        private readonly IMapper _mapper;

        public ReasonsController(IReasonService reasonService, IGenericCache<ResponseDto<ReasonDto>> cache, IMapper mapper)
        {
            _reasonService = reasonService;
            _cache = cache;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Reason}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _reasonService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Reason}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            var reasons = await _cache.GetOrCreate(CacheType.Utility, $"{filter.Page}-{filter.PageSize}-{filter.Search}", async () =>
            {
                return await _reasonService.GetAllAsync(filter);
            });
            return Ok(reasons);
        }

        //[HttpPost()]
        //[ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Reason}", permission: Permissions.Create)]
        //public async Task<IActionResult> Create(ReasonDto reason)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState.ErrorMessage());
        //    }

        //    var entity = await _reasonService.AddAsync(reason);
        //    return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        //}

        //[HttpPut()]
        //[ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Reason}", permission: Permissions.Update)]
        //public async Task<IActionResult> Update(ReasonDto reason)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    await _reasonService.UpdateAsync(reason);
        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //[ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Reason}", permission: Permissions.Delete)]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    await _reasonService.RemoveAsync(id);
        //    return NoContent();
        //}
    }
}

using AutoMapper;
using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Dtos.Inventory;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Inventory.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("inventory/[controller]")]
    [ApiController()]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _ProductService;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductCreationDto> _validator;

        public ProductsController(
            IProductService ProductService,
            IMapper mapper,
            IValidator<ProductCreationDto> validator)
        {
            _ProductService = ProductService;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _ProductService.GetProductAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _ProductService.GetProductsAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ProductCreationDto product)
        {
            var validationResult = await _validator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _ProductService.AddAsync(product);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(ProductCreationDto product)
        {
            var validationResult = await _validator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _ProductService.UpdateAsync(product);
            return NoContent();
        }

        [HttpPost("photo/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Create)]
        public async Task<IActionResult> UploadPhoto(ProductUploadPhotoDto product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _ProductService.SavePhoto(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Product}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _ProductService.RemoveAsync(id);
            return NoContent();
        }
    }
}

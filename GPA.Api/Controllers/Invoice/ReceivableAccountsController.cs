﻿using AutoMapper;
using GPA.Api.Extensions;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Invoice.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("invoice/[controller]")]
    [ApiController()]
    public class ReceivableAccountsController : ControllerBase
    {
        private readonly IReceivableAccountService _service;
        private readonly IReceivableAccountProofOfPaymentPrintService _receivableAccountProofOfPaymentPrintService;
        private readonly IMapper _mapper;

        public ReceivableAccountsController(
            IReceivableAccountService service, 
            IReceivableAccountProofOfPaymentPrintService receivableAccountProofOfPaymentPrintService, 
            IMapper mapper)
        {
            _service = service;
            _receivableAccountProofOfPaymentPrintService = receivableAccountProofOfPaymentPrintService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpGet("invoice/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> GetByInvoiceIdAsync(Guid id)
        {
            return Ok(await _service.GetByInvoiceIdAsync(id));
        }

        //[HttpGet()]
        //[ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        //public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        //{
        //    return Ok(await _service.GetAllAsync(filter));
        //}

        [HttpGet("summary")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> GetReceivableSummary([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _service.GetReceivableSummaryAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ClientPaymentsDetailCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            var entity = await _service.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(ClientPaymentsDetailCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _service.UpdateAsync(model);
            return NoContent();
        }


        [HttpGet("payments/{id}/proof-of-payment/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Print)]
        public async Task<IActionResult> PrintProofOfPaymentInvoice(Guid id)
        {
            var receivableAccountProofOfPayment = await _receivableAccountProofOfPaymentPrintService.PrintInvoice(id);
            return File(receivableAccountProofOfPayment, "application/pdf", "comprobante de pago de cuentas por cobrar.pdf");
        }
    }
}

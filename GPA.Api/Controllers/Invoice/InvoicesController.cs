using AutoMapper;
using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoices;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Invoice.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("invoice/[controller]")]
    [ApiController()]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly IValidator<InvoiceUpdateDto> _updateValidator;
        private readonly IValidator<InvoiceDto> _createValidator;
        private readonly IInvoicePrintService _invoicePrintService;
        private readonly IProofOfPaymentPrintService _proofOfPaymentPrintService;

        public InvoicesController(
            IInvoiceService invoiceService,
            IValidator<InvoiceUpdateDto> updateValidator,
            IValidator<InvoiceDto> createValidator,
            IInvoicePrintService invoicePrintService,
            IProofOfPaymentPrintService proofOfPaymentPrintService,
            IMapper mapper)
        {
            _invoiceService = invoiceService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _invoicePrintService = invoicePrintService;
            _proofOfPaymentPrintService = proofOfPaymentPrintService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _invoiceService.GetInvoiceByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _invoiceService.GetInvoicesAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(InvoiceDto invoice)
        {
            var validationResult = await _createValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            try
            {
                var entity = await _invoiceService.AddAsync(invoice);
                return Created(Url.Action(nameof(Get), new { id = entity.Id }), entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(InvoiceUpdateDto invoice)
        {
            var validationResult = await _updateValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            try
            {
                await _invoiceService.UpdateAsync(invoice);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _invoiceService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("{invoiceId}/attachment/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadAttachment(Guid invoiceId, IFormCollection files)
        {
            if (files?.Files is { Count: 0 })
            {
                return BadRequest(new string[] { "No contiene archivos para subir." });
            }

            foreach (var file in files.Files)
            {
                await _invoiceService.SaveAttachment(invoiceId, file);
            }

            return NoContent();
        }

        [HttpGet("{invoiceId}/attachments")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Read)]
        public async Task<IActionResult> GetAttachmentsAsync(Guid invoiceId)
        {
            return Ok(await _invoiceService.GetAttachmentByInvoiceIdAsync(invoiceId));
        }

        [HttpPost("attachments/{attachmentId}/download")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Download)]
        public async Task<IActionResult> DownloadFile([FromRoute] Guid attachmentId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (file, fileName) = await _invoiceService.DownloadAttachmentAsync(attachmentId);
                if (file is not null)
                {
                    return File(file, "application/octet-stream", fileName);

                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NotFound("No se encontró el archivo.");
        }

        [HttpPut("cancel/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Cancel)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _invoiceService.CancelAsync(id);
            return NoContent();
        }

        [HttpGet("{invoiceId}/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Print)]
        public async Task<IActionResult> PrintInvoice(Guid invoiceId)
        {
            var invoice = await _invoicePrintService.PrintInvoice(invoiceId);
            return File(invoice, "application/pdf", "factura.pdf");
        }

        [HttpGet("{invoiceId}/proof-of-payment/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Invoicing}", permission: Permissions.Print)]
        public async Task<IActionResult> PrintProofOfPaymentInvoice(Guid invoiceId)
        {
            var proofOfPayment = await _proofOfPaymentPrintService.PrintInvoice(invoiceId);
            return File(proofOfPayment, "application/pdf", "comprobante de pago.pdf");
        }
    }
}

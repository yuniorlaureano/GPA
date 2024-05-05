using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoices;
using GPA.Data.Invoice;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoiceService
    {
        public Task<InvoiceDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<InvoiceDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null);

        public Task<InvoiceDto?> AddAsync(InvoiceDto invoiceDto);

        public Task UpdateAsync(InvoiceDto invoiceDto);

        public Task RemoveAsync(Guid id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            var invoice = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<ResponseDto<InvoiceDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<InvoiceDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<InvoiceDto>>(categories)
            };
        }

        public async Task<InvoiceDto> AddAsync(InvoiceDto dto)
        {
            var invoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
            var savedInvoice = await _repository.AddAsync(invoice);
            return _mapper.Map<InvoiceDto>(savedInvoice);
        }

        public async Task UpdateAsync(InvoiceDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newInvoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
            newInvoice.Id = dto.Id.Value;
            var savedInvoice = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedInvoice, newInvoice, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedInvoice);
        }
    }
}

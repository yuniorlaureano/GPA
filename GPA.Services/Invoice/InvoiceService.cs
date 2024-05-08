using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoices;
using GPA.Common.DTOs.Unmapped;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoiceService
    {
        public Task<InvoiceListDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<InvoiceListDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null);

        public Task<InvoiceDto?> AddAsync(InvoiceDto invoiceDto);

        public Task UpdateAsync(InvoiceDto invoiceDto);

        public Task RemoveAsync(Guid id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository repository, IStockRepository stockRepository, IMapper mapper)
        {
            _repository = repository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<InvoiceListDto?> GetByIdAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(
                query => query.Include(x => x.Client).Include(x => x.InvoiceDetails).ThenInclude(x => x.Product)
                , x => x.Id == id);

            var invoice = _mapper.Map<InvoiceListDto>(savedInvoice);

            if (savedInvoice is { InvoiceDetails: { Count: > 0 } })
            {
                var productsId = savedInvoice.InvoiceDetails.Select(x => x.Product.Id).ToList();
                if (productsId is not null)
                {
                    var stocks = (await _stockRepository.GetProductCatalogAsync(productsId.ToArray()))
                            .ToDictionary(k => k.ProductId, v => v);

                    foreach (var invoiceDetail in invoice.InvoiceDetails)
                    {
                        if (stocks.TryGetValue(invoiceDetail.ProductId, out var product))
                        {
                            invoiceDetail.StockProduct = _mapper.Map<RawProductCatalogDto>(product);
                        }
                    }
                }
            }

            return invoice;
        }

        public async Task<ResponseDto<InvoiceListDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<InvoiceListDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<InvoiceListDto>>(categories)
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

using GPA.Entities.Unmapped.Invoice;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Invoice
{
    public interface IInvoicePrintRepository
    {
        Task<RawInvoice?> GetInvoiceById(Guid invoiceId);
        Task<List<PrintRawInvoiceDetails>> GetInvoiceDetailByInvoiceId(Guid invoiceId);
        Task<Dictionary<Guid, List<RawInvoiceDetailsAddon>>> GetInvoiceDetailAddonByInvoiceId(Guid invoiceId);
    }

    public class InvoicePrintRepository : IInvoicePrintRepository
    {
        private readonly GPADbContext _context;

        public InvoicePrintRepository(GPADbContext _dbContext)
        {
            _context = _dbContext;
        }

        public async Task<RawInvoice?> GetInvoiceById(Guid invoiceId)
        {
            var query = @$"
                SELECT 
	                 INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.[Code]
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                FROM [GPA].[Invoice].[Invoices] INV
                WHERE INV.[Id] = @Id
                    ";

            return await _context.Database.SqlQueryRaw<RawInvoice>(query, new SqlParameter("@Id", invoiceId)).FirstOrDefaultAsync();
        }

        public async Task<List<PrintRawInvoiceDetails>> GetInvoiceDetailByInvoiceId(Guid invoiceId)
        {
            var query = @$"
                SELECT 
                       INVD.[Id]
                      ,INVD.[Price]
                      ,INVD.[Quantity]
                      ,INVD.[ProductId]
                      ,INVD.[InvoiceId]
	                  ,PRO.[Name] AS ProductName
	                  ,PRO.Code AS ProductCode
                  FROM [GPA].[Invoice].[InvoiceDetails] INVD
	                   JOIN [GPA].[Inventory].[Products] PRO ON PRO.Id = INVD.ProductId
              WHERE [InvoiceId] = @Id 
                    ";

            return await _context.Database.SqlQueryRaw<PrintRawInvoiceDetails>(query, new SqlParameter("@Id", invoiceId)).ToListAsync();
        }

        public async Task<Dictionary<Guid, List<RawInvoiceDetailsAddon>>> GetInvoiceDetailAddonByInvoiceId(Guid invoiceId)
        {
            var query = @$"
                SELECT INVDADD.[Id]
                      ,INVDADD.[Concept]
                      ,INVDADD.[IsDiscount]
                      ,INVDADD.[Type]
                      ,INVDADD.[Value]
                      ,INVDADD.[InvoiceDetailId]
                  FROM [GPA].[Invoice].[InvoiceDetailsAddons] INVDADD
	                   JOIN [GPA].[Invoice].[InvoiceDetails] INVD 
			                ON INVD.InvoiceId = @Id
				                AND INVD.Id = INVDADD.InvoiceDetailId
                    ";

            var invoiceDetailsAddons = await _context.Database.SqlQueryRaw<RawInvoiceDetailsAddon>(query, new SqlParameter("@Id", invoiceId)).ToListAsync();

            var invoiceDetailsAddonAsDictionary = new Dictionary<Guid, List<RawInvoiceDetailsAddon>>();
            foreach (var detailAddon in invoiceDetailsAddons)
            {
                if (!invoiceDetailsAddonAsDictionary.ContainsKey(detailAddon.InvoiceDetailId))
                {
                    invoiceDetailsAddonAsDictionary.Add(detailAddon.InvoiceDetailId, new());
                }

                invoiceDetailsAddonAsDictionary[detailAddon.InvoiceDetailId].Add(detailAddon);
            }

            return invoiceDetailsAddonAsDictionary;
        }
    }
}

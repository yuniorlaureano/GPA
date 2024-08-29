using GPA.Common.Entities.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Invoice;

namespace GPA.Utils
{
    public class PaymentCalculator
    {
        public static decimal GetNetPrice(InvoiceDetails detail, Dictionary<Guid, List<RawAddons>> addons)
        {
            if (addons.ContainsKey(detail.ProductId))
            {
                var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                return detail.Price - debit + credit;
            }
            return detail.Price;
        }

        public static void CheckIfClientHasEnoughCredit(IEnumerable<RawPenddingPayment> debits, IEnumerable<RawCredit> credits, decimal payment, ICollection<InvoiceDetails> invoiceDetails, Dictionary<Guid, List<RawAddons>> addons)
        {
            var debit = debits.Sum(x => x.PendingPayment);
            var credit = credits.Sum(x => x.Credit);
            var toPay = invoiceDetails.Sum(x => x.Quantity * GetNetPrice(x, addons));
            var duePayment = toPay - payment;

            if (duePayment > (credit - debit))
            {
                throw new InvalidOperationException("No se puede proceder con la venta, el cliente no tiene suficiente credito");
            }
        }

        public static PaymentStatus GetPaymentStatus(Invoice invoice, Dictionary<Guid, List<RawAddons>> addons)
        {
            if (invoice.Payment < invoice.InvoiceDetails.Sum(x => x.Quantity * GetNetPrice(x, addons)))
            {
                return PaymentStatus.Pending;
            }

            return PaymentStatus.Payed;
        }

        public static PaymentStatus GetPaymentStatus(Invoice invoice, List<InvoiceDetails> invoiceDetails, Dictionary<Guid, List<RawAddons>> addons)
        {
            if (invoice.Payment < invoiceDetails.Sum(x => x.Quantity * GetNetPrice(x, addons)))
            {
                return PaymentStatus.Pending;
            }

            return PaymentStatus.Payed;
        }
    }
}

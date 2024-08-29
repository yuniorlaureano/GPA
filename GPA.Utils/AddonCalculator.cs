using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Invoice;

namespace GPA.Utils
{
    public class AddonCalculator
    {
        public static (decimal debit, decimal credit) CalculateAddon(decimal price, Addon[] addons)
        {
            var debit = 0M;
            var credit = 0M;
            foreach (var addon in addons)
            {
                if (addon.IsDiscount)
                {
                    debit += CalculateAmount(addon.Type, addon.Value);
                    debit += CalculatePercentage(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentage(addon.Type, addon.Value, price);
                }
            }
            return (debit, credit);
        }

        public static (decimal debit, decimal credit) CalculateAddon(decimal price, AddonDto[] addons)
        {
            var debit = 0M;
            var credit = 0M;
            foreach (var addon in addons)
            {
                if (addon.IsDiscount)
                {
                    debit += CalculateAmount(addon.Type, addon.Value);
                    debit += CalculatePercentage(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentage(addon.Type, addon.Value, price);
                }
            }
            return (debit, credit);
        }

        public static (decimal debit, decimal credit) CalculateAddon(decimal price, List<RawAddons> addons)
        {
            var debit = 0M;
            var credit = 0M;
            foreach (var addon in addons)
            {
                if (addon.IsDiscount)
                {
                    debit += CalculateAmount(addon.Type, addon.Value);
                    debit += CalculatePercentage(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentage(addon.Type, addon.Value, price);
                }
            }
            return (debit, credit);
        }

        public static decimal CalculateAmount(string type, decimal value)
        {
            return type == AddonsType.AMOUNT ? value : 0M;
        }

        public static decimal CalculatePercentage(string type, decimal value, decimal price)
        {
            return type == AddonsType.PERCENTAGE ? (price * value / 100M) : 0M;
        }

        public static decimal CalculateAmountOrPercentage(RawAddons rawAddons, decimal price)
        {
            var result = CalculateAmount(rawAddons.Type, rawAddons.Value) + CalculatePercentage(rawAddons.Type, rawAddons.Value, price);
            if(rawAddons.IsDiscount)
            {
                return -result;
            }
            return result;
        }

        public static decimal CalculateAmountOrPercentage(RawInvoiceDetailsAddon rawInvoiceDetailsAddon, decimal price)
        {
            return CalculateAmount(rawInvoiceDetailsAddon.Type, rawInvoiceDetailsAddon.Value) + CalculatePercentage(rawInvoiceDetailsAddon.Type, rawInvoiceDetailsAddon.Value, price);
        }

        public static (decimal debit, decimal credit) CalculateAddon(decimal price, List<RawInvoiceDetailsAddon> addons)
        {
            var debit = 0M;
            var credit = 0M;
            foreach (var addon in addons)
            {
                if (addon.IsDiscount)
                {
                    debit += CalculateAmount(addon.Type, addon.Value);
                    debit += CalculatePercentage(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentage(addon.Type, addon.Value, price);
                }
            }
            return (debit, credit);
        }
    }
}

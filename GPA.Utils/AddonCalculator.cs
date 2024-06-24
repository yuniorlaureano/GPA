using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped;

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
                    debit += CalculatePercentaje(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentaje(addon.Type, addon.Value, price);
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
                    debit += CalculatePercentaje(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentaje(addon.Type, addon.Value, price);
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
                    debit += CalculatePercentaje(addon.Type, addon.Value, price);
                }
                else
                {
                    credit += CalculateAmount(addon.Type, addon.Value);
                    credit += CalculatePercentaje(addon.Type, addon.Value, price);
                }
            }
            return (debit, credit);
        }

        public static decimal CalculateAmount(string type, decimal value)
        {
            return type == AddonsType.AMOUNT ? value : 0M;
        }

        public static decimal CalculatePercentaje(string type, decimal value, decimal price)
        {
            return type == AddonsType.PERCENTAGE ? (price * value / 100M) : 0M;
        }
    }
}

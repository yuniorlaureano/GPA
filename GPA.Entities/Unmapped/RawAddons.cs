
using GPA.Entities.Common;

namespace GPA.Entities.Unmapped
{
    public class RawAddons
    {
        public Guid? Id { get; set; }
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public Guid ProductId { get; set; }
    }
}

using GPA.Entities.General;

namespace GPA.Entities.Unmapped.General
{
    public class RawInputVsOutputVsExistence
    {
        public int Input { get; set; }
        public int Output { get; set; }
        public int Existence { get; set; }
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
        public ProductType ProductType { get; set; }
    }
}

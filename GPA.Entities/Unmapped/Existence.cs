
using GPA.Entities.General;

namespace GPA.Entities.Unmapped
{
    public class Existence
    {
        public int Stock { get; set; }
        public int Input { get; set; }
        public int Output { get; set; }
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public ProductType ProductType { get; set; }
        public String ProductName { get; set; }
        public String ProductCode { get; set; }
        public Guid CategoryId { get; set; }
    }
}

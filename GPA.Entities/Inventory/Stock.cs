using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Stock : Entity
    {
        public string Code { get; set; }
        public int Input { get; set; }
        public int OutInput { get; set; }


        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}

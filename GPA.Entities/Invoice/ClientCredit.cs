using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class ClientCredit : Entity<Guid>
    {
        public float Credit { get; set; }
        public string Concept { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; }
    }
}

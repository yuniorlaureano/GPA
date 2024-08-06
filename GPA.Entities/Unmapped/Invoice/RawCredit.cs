namespace GPA.Entities.Unmapped.Invoice
{
    public class RawCredit
    {
        public Guid Id { get; set; }
        public decimal Credit { get; set; }
        public string Concept { get; set; }
        public Guid ClientId { get; set; }
    }
}

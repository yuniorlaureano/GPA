namespace GPA.Common.DTOs.Invoice
{
    public class ClientCreditDto
    {
        public float Credit { get; set; }
        public string Concept { get; set; }
        public Guid ClientId { get; set; }
    }
}

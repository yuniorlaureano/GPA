namespace GPA.Entities.Unmapped.Audit
{
    public class AddonHistory : EntityHistory<Guid>
    {
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }        
    }
}

namespace GPA.Entities.Unmapped.Audit
{
    public class ProductAddonHistory : EntityHistory<Guid>
    {
        public string Concept { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}

namespace GPA.Entities.Report
{
    public class ReportTemplate
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Template { get; set; }
    }
}

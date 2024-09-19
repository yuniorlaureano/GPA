namespace GPA.Entities.Report
{
    public class ReportTemplate
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Template { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}

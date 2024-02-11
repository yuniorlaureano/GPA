namespace GPA.Dtos
{
    public abstract class Dto
    {
        public Guid Id { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}

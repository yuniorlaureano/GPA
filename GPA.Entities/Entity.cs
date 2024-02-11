namespace GPA.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool Deleted { get; set; }
    }
}

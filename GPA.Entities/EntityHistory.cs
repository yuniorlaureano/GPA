namespace GPA.Entities
{
    public abstract class EntityHistory<Key>
    {
        public Guid Id { get; set; }
        public Key IdentityId { get; set; }
        public string Action { get; set; }
        public Guid By { get; set; }
        public DateTimeOffset At { get; set; }
    }
}

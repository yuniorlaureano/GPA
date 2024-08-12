namespace GPA.Entities.General
{
    public class BlobStorageConfiguration : Entity<Guid>
    {
        public string Identifier { get; set; }
        public string Provider { get; set; }
        public string Value { get; set; }
        public bool Current { get; set; }
    }
}

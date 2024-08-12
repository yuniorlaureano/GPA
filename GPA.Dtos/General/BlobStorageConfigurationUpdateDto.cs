namespace GPA.Dtos.General
{
    public class BlobStorageConfigurationUpdateDto
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public string Provider { get; set; }
        public string Value { get; set; }
        public bool Current { get; set; }
    }
}

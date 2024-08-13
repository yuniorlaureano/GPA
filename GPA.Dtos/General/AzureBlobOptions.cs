namespace GPA.Dtos.General
{
    public class AzureBlobOptions : IGPABlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
    }
}

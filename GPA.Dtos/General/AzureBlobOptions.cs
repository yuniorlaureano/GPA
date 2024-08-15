namespace GPA.Dtos.General
{
    public class AzureBlobOptions : IGPABlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string PublicContainer { get; set; }
        public string PrivateContainer { get; set; }
    }
}

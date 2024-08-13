namespace GPA.Dtos.General
{
    public class GCPBucketOptions : IGPABlobStorageOptions
    {
        public string JsonCredentials { get; set; }
        public string Bucket { get; set; }
    }
}

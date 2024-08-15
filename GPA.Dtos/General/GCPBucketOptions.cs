namespace GPA.Dtos.General
{
    public class GCPBucketOptions : IGPABlobStorageOptions
    {
        public string JsonCredentials { get; set; }
        public string PublicBucket { get; set; }
        public string PrivateBucket { get; set; }
    }
}

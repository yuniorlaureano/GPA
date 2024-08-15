namespace GPA.Dtos.General
{
    public class AWSS3Options : IGPABlobStorageOptions
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string PublicBucket { get; set; }
        public string PrivateBucket { get; set; }
        public string Region { get; set; }
    }
}

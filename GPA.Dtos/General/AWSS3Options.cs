namespace GPA.Dtos.General
{
    public class AWSS3Options : IGPABlobStorageOptions
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Bucket { get; set; }
        public string Region { get; set; }
    }
}

namespace GPA.Dtos.General
{
    public class BlobStorageFileResult
    {
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public string Provider { get; set; }
        public string BucketOrContainer { get; set; }
    }
}

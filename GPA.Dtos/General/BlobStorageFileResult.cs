namespace GPA.Dtos.General
{
    public class BlobStorageFileResult
    {
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public long FileSize { get; set; }
        public string Provider { get; set; }
    }
}

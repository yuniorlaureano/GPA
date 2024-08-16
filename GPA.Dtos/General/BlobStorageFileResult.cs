using System.Text.Json;

namespace GPA.Dtos.General
{
    public class BlobStorageFileResult
    {
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public string FileUrl { get; set; }

        public string AsJson()
        {

           return JsonSerializer.Serialize(this, new JsonSerializerOptions
           {
               PropertyNamingPolicy = JsonNamingPolicy.CamelCase
           });
        }
    }
}

namespace FileUploadApp.Models
{
    public class UploadResponse
    {
        public string Message { get; set; }
        public IEnumerable<string> JsonFilePaths { get; set; }
        public IEnumerable<string> Warnings { get; set; }
    }
}

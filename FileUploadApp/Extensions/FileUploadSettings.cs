namespace FileUploadApp.Extensions
{
    public class FileUploadSettings
    {
        public string? TargetDirectory { get; set; }
        public string? JsonOutputPath { get; set; }
        public string? UploadEndpointPath { get; set; }
        public int KestrelMaxRequestBodySize { get; set; }
    }
}

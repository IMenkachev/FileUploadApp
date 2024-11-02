namespace FileUploadApp.Contracts
{
    public interface IXmlToJsonConverter
    {
        Task<string> ConvertXmlToJsonAsync(string xmlFilePath, string outputDirectory);
    }
}

using FileUploadApp.Contracts;
using Newtonsoft.Json;
using System.Xml;

namespace FileUploadApp.Services
{
    public class XmlToJsonConverter : IXmlToJsonConverter
    {
        public async Task<string> ConvertXmlToJsonAsync(string xmlFilePath, string outputDirectory)
        {
            var jsonFileName = Path.ChangeExtension(Path.GetFileName(xmlFilePath), ".json");
            var jsonFilePath = Path.Combine(outputDirectory, jsonFileName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            await using var stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var xmlContent = await reader.ReadToEndAsync();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            var jsonText = JsonConvert.SerializeXmlNode(xmlDoc);

            await File.WriteAllTextAsync(jsonFilePath, jsonText);

            return jsonFilePath;
        }
    }
}

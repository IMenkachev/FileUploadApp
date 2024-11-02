using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FileUploadApp.Controllers;
using FileUploadApp.Contracts;
using FileUploadApp.Models;
using FileUploadApp.Extensions;

namespace FileUploadAppTests
{
    public class FileUploadControllerTests
    {
        private readonly Mock<IOptions<FileUploadSettings>> _fileUploadSettingsMock;
        private readonly Mock<ILogger<FileUploadController>> _loggerMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<IXmlToJsonConverter> _xmlToJsonConverterMock;
        private readonly Mock<IRetryPolicy> _retryPolicyMock;
        private readonly FileUploadController _controller;

        public FileUploadControllerTests()
        {
            _fileUploadSettingsMock = new Mock<IOptions<FileUploadSettings>>();
            _loggerMock = new Mock<ILogger<FileUploadController>>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _xmlToJsonConverterMock = new Mock<IXmlToJsonConverter>();
            _retryPolicyMock = new Mock<IRetryPolicy>();

            var settings = new FileUploadSettings
            {
                TargetDirectory = "test-directory",
                JsonOutputPath = "test-json-output"
            };
            _fileUploadSettingsMock.Setup(s => s.Value).Returns(settings);

            _controller = new FileUploadController(
                _fileUploadSettingsMock.Object,
                _loggerMock.Object,
                _fileStorageServiceMock.Object,
                _xmlToJsonConverterMock.Object,
                _retryPolicyMock.Object
            );
        }

        [Fact]
        public async Task UploadMultipleXmlFiles_ShouldReturnOk_WhenAllFilesAreProcessedSuccessfully()
        {
            // Arrange
            var files = new List<IFormFile>
            {
                CreateMockFile("<root>Valid XML</root>", "valid1.xml"),
                CreateMockFile("<root>Another Valid XML</root>", "valid2.xml")
            };

            _fileStorageServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                                   .ReturnsAsync("savedFilePath");
            _xmlToJsonConverterMock.Setup(c => c.ConvertXmlToJsonAsync(It.IsAny<string>(), It.IsAny<string>()))
                                   .ReturnsAsync("convertedJsonPath");

            // Act
            var result = await _controller.UploadMultipleXmlFiles(files) as OkObjectResult;
            var response = result.Value as UploadResponse;

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal("All files uploaded and processed successfully.", response.Message);
            Assert.NotEmpty(response.JsonFilePaths);
        }

        private IFormFile CreateMockFile(string content, string fileName)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.Length).Returns(bytes.Length);
            mockFile.Setup(f => f.FileName).Returns(fileName);

            return mockFile.Object;
        }
    }
}

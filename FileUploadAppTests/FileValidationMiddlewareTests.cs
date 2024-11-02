
namespace FileUploadAppTests
{
    using FileUploadApp.Extensions;
    using FileUploadApp.Middlewares;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit;

    public class FileValidationMiddlewareTests
    {
        private readonly FileValidationMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<IOptions<FileUploadSettings>> _optionsMock;
        private readonly Mock<ILogger<FileValidationMiddleware>> _loggerMock;

        public FileValidationMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _optionsMock = new Mock<IOptions<FileUploadSettings>>();
            _loggerMock = new Mock<ILogger<FileValidationMiddleware>>();

            // Configure options mock, if needed
            var options = new FileUploadSettings { /* Set properties if any */ };
            _optionsMock.Setup(o => o.Value).Returns(options);

            // Pass all required dependencies into the middleware constructor
            _middleware = new FileValidationMiddleware(_nextMock.Object, _optionsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessRequest_ShouldReturnError_WhenFileIsEmpty()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            mockFile.Setup(f => f.FileName).Returns("empty.xml");

            var result = _middleware.IsValidXmlFile(mockFile.Object, out var errorMessage);

            Assert.False(result);
            Assert.Equal("The file 'empty.xml' is empty.", errorMessage);
        }

        [Fact]
        public async Task ProcessRequest_ShouldReturnError_WhenFileIsNotXml()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("notXml.txt");

            var result = _middleware.IsValidXmlFile(mockFile.Object, out var errorMessage);

            Assert.False(result);
            Assert.Equal("The file 'notXml.txt' is not an XML file.", errorMessage);
        }

        [Fact]
        public async Task ProcessRequest_ShouldReturnError_WhenFileIsMalformedXml()
        {
            var malformedXmlFile = CreateMockFile("<invalid><xml>", "test.xml");

            var result = _middleware.IsValidXmlFile(malformedXmlFile, out var errorMessage);

            Assert.False(result);
            Assert.Contains("not a well-formed XML", errorMessage);
        }

        [Fact]
        public async Task ProcessRequest_ShouldPass_WhenFileIsValidXml()
        {
            var validXmlFile = CreateMockFile("<root><test>Valid XML</test></root>", "valid.xml");

            var result = _middleware.IsValidXmlFile(validXmlFile, out var errorMessage);

            Assert.True(result);
            Assert.Null(errorMessage);
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

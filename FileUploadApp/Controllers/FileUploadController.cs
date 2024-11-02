using Microsoft.AspNetCore.Mvc;
using FileUploadApp.Contracts;
using System.Collections.Concurrent;
using FileUploadApp.Extensions;
using Microsoft.Extensions.Options;
using FileUploadApp.Models;

namespace FileUploadApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : Controller
    {
        private readonly string _targetDirectory;
        private readonly string? _jsonOutputPath;
        private readonly ILogger<FileUploadController> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IXmlToJsonConverter _xmlToJsonConverter;
        private readonly IRetryPolicy _retryPolicy;

        public FileUploadController(
            IOptions<FileUploadSettings> fileUploadSettings,
            ILogger<FileUploadController> logger,
            IFileStorageService fileStorageService,
            IXmlToJsonConverter xmlToJsonConverter,
            IRetryPolicy retryPolicy)
        {
            var settings = fileUploadSettings.Value;
            _targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), settings.TargetDirectory);
            _jsonOutputPath = settings.JsonOutputPath;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _xmlToJsonConverter = xmlToJsonConverter;
            _retryPolicy = retryPolicy;

            if (!Directory.Exists(_targetDirectory))
            {
                Directory.CreateDirectory(_targetDirectory);
            }
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleXmlFiles(List<IFormFile> files)
        {
            var logTimestamp = DateTime.UtcNow;
            var uploadResults = new ConcurrentBag<string>();
            var warnings = new List<string>();
            var tasks = new List<Task>();

            foreach (var file in files)
            {
                tasks.Add(Task.Run(() => ProcessFileAsync(file, uploadResults, logTimestamp)));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during the file upload process: {ErrorMessage} - {Timestamp}", ex.Message, logTimestamp);
                return StatusCode(500, new UploadResponse
                {
                    Message = "An error occurred while processing the files."
                });
            }

            return Ok(new UploadResponse
            {
                Message = "All files uploaded and processed successfully.",
                JsonFilePaths = uploadResults,
                Warnings = warnings
            });
        }


        private async Task ProcessFileAsync(IFormFile file, ConcurrentBag<string> uploadResults, DateTime logTimestamp)
        {
            try
            {
                // Save file with retry
                var filePath = await _retryPolicy.ExecuteAsync(() =>
                    _fileStorageService.SaveFileAsync(file, _targetDirectory));

                // Convert XML to JSON with retry
                var jsonFilePath = await _retryPolicy.ExecuteAsync(() =>
                    _xmlToJsonConverter.ConvertXmlToJsonAsync(filePath, _jsonOutputPath));

                // Add the result to the ConcurrentBag
                uploadResults.Add(jsonFilePath);

                _logger.LogInformation("File {FileName} processed successfully - {Timestamp}", file.FileName, logTimestamp);
            }
            catch (IOException ex) // File system issues
            {
                _logger.LogError("File system error for file {FileName}: {ErrorMessage} - {Timestamp}", file.FileName, ex.Message, logTimestamp);
            }
            catch (UnauthorizedAccessException ex) // Permission issues
            {
                _logger.LogError("Permission error for file {FileName}: {ErrorMessage} - {Timestamp}", file.FileName, ex.Message, logTimestamp);
            }
        }
    }
}

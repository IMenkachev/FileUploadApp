using FileUploadApp.Extensions;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Xml;

namespace FileUploadApp.Middlewares
{
    public class FileValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FileUploadSettings _options;
        private readonly ILogger<FileValidationMiddleware> _logger;

        public FileValidationMiddleware(RequestDelegate next,
            IOptions<FileUploadSettings> options,
            ILogger<FileValidationMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await IsRequestBodyTooLarge(context);
                await HasUploadedFiles(context);
                await HasInvalidXmlFiles(context);

                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = $"An error occurred: {ex.Message}" });
                _logger.LogError(ex, "Unexpected error in request processing: {ErrorMessage} - {Timestamp}", ex.Message, DateTime.UtcNow);
            }
        }

        private async Task IsRequestBodyTooLarge(HttpContext context)
        {
            if (context.Request.ContentLength > _options.KestrelMaxRequestBodySize)
            {
                context.Response.StatusCode = StatusCodes.Status413RequestEntityTooLarge;
                await context.Response.WriteAsJsonAsync(new { message = "Total file size exceeds the allowed limit of 50 MB." });
                _logger.LogWarning("Total uploaded file size exceeds limit of 50 MB - {Timestamp}", DateTime.UtcNow);
                return;
            }
        }

        private async Task HasUploadedFiles(HttpContext context)
        {
            if (!context.Request.HasFormContentType || !context.Request.Form.Files.Any())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = "No files uploaded. Please select at least one XML file." });
                _logger.LogWarning("No files selected for upload - {Timestamp}", DateTime.UtcNow);
                return;
            }
        }

        private async Task HasInvalidXmlFiles(HttpContext context)
        {
            var errorMessages = new ConcurrentBag<string>();
            var tasks = context.Request.Form.Files.Select(file => Task.Run(() =>
            {
                if (!IsValidXmlFile(file, out string errorMessage))
                {
                    errorMessages.Add(errorMessage);
                }
            })).ToList();

            await Task.WhenAll(tasks);

            if (errorMessages.Any())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = errorMessages, errors = errorMessages });
                _logger.LogWarning("An error acurred: {ErrorMessage} - {Timestamp}", errorMessages, DateTime.UtcNow);
                return;
            }
        }

        public bool IsValidXmlFile(IFormFile file, out string errorMessage)
        {
            errorMessage = null;

            if (file.Length == 0)
            {
                errorMessage = $"The file '{file.FileName}' is empty.";
                return false;
            }

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"The file '{file.FileName}' is not an XML file.";
                return false;
            }

            // Validate XML format
            try
            {
                using var stream = file.OpenReadStream();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(stream); // This will throw an exception if the XML is invalid
            }
            catch (XmlException ex)
            {
                errorMessage = $"The file '{file.FileName}' is not a well-formed XML: {ex.Message}";
                return false;
            }

            return true;
        }

    }
}
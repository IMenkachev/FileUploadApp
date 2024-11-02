using System.Net;

namespace FileUploadApp.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (!context.Response.HasStarted)
                {
                    _logger.LogError(ex, "An unhandled exception has occurred: {Error}", ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
                }
                else
                {
                    _logger.LogError("Unable to set response status code because the response has already started: {Error}", ex.Message);
                }
            }
        }

    }
}

using FileUploadApp.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileUploadAppTests
{
    public class ErrorHandlingMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly ErrorHandlingMiddleware _middleware;
        private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;

        public ErrorHandlingMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            _middleware = new ErrorHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldSetErrorResponse_WhenExceptionIsThrown()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Test exception"));
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(context);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }
    }
}

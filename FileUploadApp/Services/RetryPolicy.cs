using FileUploadApp.Contracts;
using Polly;

namespace FileUploadApp.Services
{
    public class RetryPolicy : IRetryPolicy
    {
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            return await policy.ExecuteAsync(action);
        }
    }
}

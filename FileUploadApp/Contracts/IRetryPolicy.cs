namespace FileUploadApp.Contracts
{
    public interface IRetryPolicy
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);
    }
}

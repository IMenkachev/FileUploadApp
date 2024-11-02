namespace FileUploadApp.Contracts
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string directory);
    }
}

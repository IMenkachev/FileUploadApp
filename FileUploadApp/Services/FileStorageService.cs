using FileUploadApp.Contracts;

namespace FileUploadApp.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> SaveFileAsync(IFormFile file, string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, file.FileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }
    }
}

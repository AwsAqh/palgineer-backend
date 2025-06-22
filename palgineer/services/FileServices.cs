using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class FileServices
{
    private const string UploadsFolderName = "uploads";

    public async Task<string?> saveFileAsync(string userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        // Base uploads folder
        var appRoot = Directory.GetCurrentDirectory();
        var uploadsRoot = Path.Combine(appRoot, UploadsFolderName);

         // Per-user folder
        var userFolder = Path.Combine(uploadsRoot, userId);
        if (!Directory.Exists(userFolder))
            Directory.CreateDirectory(userFolder);

        
        var fileName = Path.GetFileName(file.FileName);
        var physicalPath = Path.Combine(userFolder, fileName);

       
        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        
        return $"/{UploadsFolderName}/{userId}/{fileName}";
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public static class FileHelper
{
    private static readonly string UploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

    public static async Task<string> SaveImageAsync(IFormFile image)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image), "Image cannot be null.");
        }

        if (image.ContentType != "image/jpeg" && image.ContentType != "image/png")
        {
            throw new InvalidOperationException("Unsupported image format. Only JPEG and PNG are allowed.");
        }

        Directory.CreateDirectory(UploadFolder); 

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
        var filePath = Path.Combine(UploadFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return Path.Combine("uploads", uniqueFileName).Replace("\\", "/");
    }

    public static void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
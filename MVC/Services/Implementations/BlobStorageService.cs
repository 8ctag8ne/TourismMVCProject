using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using TourismMVCProject.Services.Interfaces;

namespace TourismMVCProject.Services.Implementations
{
    public class BlobStorageService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:ContainerName"];
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string?> UploadAsync(IFormFile? file, string prefix)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Створюємо унікальне ім'я для файлу
        string fileName = string.Join("", file.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
        string blobName = $"{prefix}/{Guid.NewGuid()}_{fileName}";

        var blobClient = containerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        // Отримуємо ім'я blob з URL
        var uri = new Uri(fileUrl);
        var blobName = uri.Segments[^1];
        
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public string GetFullUrl(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return blobClient.Uri.ToString();
    }
    }
}
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using FutureCareer.Models;
using FutureCareer.Services;
using Azure;

namespace FutureCareer.Repository
{
    public class AzureStorage : IAzureStorage
    {
        #region Dependency Injection / Constructor

        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly ILogger<AzureStorage> _logger;

        public AzureStorage(IConfiguration configuration, ILogger<AzureStorage> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _logger = logger;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
        {
            // Create new upload response object that we can return to the requesting method
            BlobResponseDto response = new();

            // Get a reference to a container named in appsettings.json and then create it
            BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            //await container.CreateAsync();
            try
            {
                // Get a reference to the blob just uploaded from the API in a container from configuration settings
                BlobClient client = container.GetBlobClient(blob.FileName);

                // Open a stream for the file we want to upload
                await using (Stream? data = blob.OpenReadStream())
                {
                    // Upload the file async
                    await client.UploadAsync(data);
                }

                // Everything is OK and file got uploaded
                response.Status = $"File {blob.FileName} Uploaded Successfully";
                response.Error = false;
                response.Blob.Uri = client.Uri.AbsoluteUri;
                response.Blob.Name = client.Name;

            }
            // If the file already exists, we catch the exception and do not upload it
            catch (RequestFailedException ex)
               when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                _logger.LogError($"File with name {blob.FileName} already exists in container. Set another name to store the file in the container: '{_storageContainerName}.'");
                response.Status = $"File with name {blob.FileName} already exists. Please use another name to store your file.";
                response.Error = true;
                return response;
            }
            // If we get an unexpected error, we catch it here and return the error message
            catch (RequestFailedException ex)
            {
                // Log error to console and create a new response we can return to the requesting method
                _logger.LogError($"Unhandled Exception. ID: {ex.StackTrace} - Message: {ex.Message}");
                response.Status = $"Unexpected error: {ex.StackTrace}. Check log with StackTrace ID.";
                response.Error = true;
                return response;
            }

            // Return the BlobUploadResponse object
            return response;
        }

        #endregion

    }
}

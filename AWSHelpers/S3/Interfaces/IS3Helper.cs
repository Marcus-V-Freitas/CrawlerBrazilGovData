using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AWSHelpers.S3.Interfaces
{
    public interface IS3Helper
    {
        Task<PutBucketResponse> CreateABucket(string bucketName);

        Task<bool> UploadFileAsync(string bucketName, Stream fileStream, string fileName, string directory = null);

        Task<bool> UploadFileAsync(string bucketName, string filePath, string directory = null);

        Task<bool> UploadFileAsync(string bucketName, string contents, string contentType, string fileName, string directory = null);

        Task<(Stream FileStream, string ContentType)> ReadFileAsync(string bucketName, string fileName, string directory = null);

        Task<List<(Stream FileStream, string fileName, string ContentType)>> ReadDirectoryAsync(string bucketName, string directory);

        Task<bool> MoveFileAsync(string bucketName, string fileName, string sourceDirectory, string destDirectory);

        Task<bool> RemoveFileAsync(string buckName, string fileName, string directory = null);

        Task<ListBucketsResponse> ListingBuckets();
    }
}